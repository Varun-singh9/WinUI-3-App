using System.Diagnostics;
using Launcher.Core.Diagnostics;
using Launcher.Core.Integrations;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// The single application runtime coordinator behind <see cref="IApplicationRuntime"/> (Implementation Specification
/// sections 5 and 6, ADR-0026). It starts and stops the pieces in the documented order and turns hotkey presses and
/// second-instance signals into activation requests. It uses only Core interfaces; the Win32 hotkey and the
/// single-instance objects are implemented in <c>Launcher.Integrations</c> and passed in by the composition root.
/// </summary>
/// <remarks>
/// <para>
/// Start order: subscribe to the hotkey and second-instance events first, so no press is missed; acquire single-instance
/// ownership; register the hotkey; start the optional integrations in the background; stay Hidden. A start problem that
/// the launcher can live with, such as a hotkey that is already in use or a single-instance failure, is reported in the
/// <see cref="ApplicationStartResult"/> and logged, and does not stop the launcher.
/// </para>
/// <para>
/// Optional integrations are started on a background task that is never awaited by activation, so they can never block
/// it. Shutdown cancels them and waits only a short, bounded time for them to stop.
/// </para>
/// <para>
/// Start, shutdown and restart are serialized. Shutdown is idempotent. Restart is an in-process shutdown followed by a
/// start. Cancelling start cleans up whatever it had already done.
/// </para>
/// </remarks>
public sealed class ApplicationRuntime : IApplicationRuntime
{
    private const string Component = "Runtime";
    private static readonly TimeSpan DefaultIntegrationShutdownGrace = TimeSpan.FromSeconds(2);

    private readonly ApplicationRuntimeOptions _options;
    private readonly ILauncherLifecycle _lifecycle;
    private readonly IActivationCoordinator _activation;
    private readonly IHotkeyService _hotkey;
    private readonly ISingleInstanceOwner _singleInstance;
    private readonly IDiagnostics _diagnostics;
    private readonly IIntegrationManager? _integrations;
    private readonly TimeSpan _integrationShutdownGrace;
    private readonly SemaphoreSlim _operation = new(1, 1);
    private readonly Lock _gate = new();

    private CancellationTokenSource? _run;
    private Task? _integrationsTask;
    private ApplicationStartResult? _started;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRuntime"/> class.
    /// </summary>
    /// <param name="options">The minimal startup configuration.</param>
    /// <param name="lifecycle">The lifecycle that <see cref="Lifecycle"/> exposes.</param>
    /// <param name="activation">Activates and deactivates the launcher.</param>
    /// <param name="hotkey">The global hotkey service.</param>
    /// <param name="singleInstance">Single-instance ownership and the second-instance signal.</param>
    /// <param name="diagnostics">Where startup, shutdown and failures are written.</param>
    /// <param name="integrations">The optional integrations, or <see langword="null"/> while none exist yet.</param>
    public ApplicationRuntime(
        ApplicationRuntimeOptions options,
        ILauncherLifecycle lifecycle,
        IActivationCoordinator activation,
        IHotkeyService hotkey,
        ISingleInstanceOwner singleInstance,
        IDiagnostics diagnostics,
        IIntegrationManager? integrations = null)
        : this(options, lifecycle, activation, hotkey, singleInstance, diagnostics, integrations, DefaultIntegrationShutdownGrace)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRuntime"/> class with a chosen integration shutdown grace period (for tests).
    /// </summary>
    /// <param name="options">The minimal startup configuration.</param>
    /// <param name="lifecycle">The lifecycle that <see cref="Lifecycle"/> exposes.</param>
    /// <param name="activation">Activates and deactivates the launcher.</param>
    /// <param name="hotkey">The global hotkey service.</param>
    /// <param name="singleInstance">Single-instance ownership and the second-instance signal.</param>
    /// <param name="diagnostics">Where startup, shutdown and failures are written.</param>
    /// <param name="integrations">The optional integrations, or <see langword="null"/>.</param>
    /// <param name="integrationShutdownGrace">How long shutdown waits for the integrations to stop after cancelling them.</param>
    internal ApplicationRuntime(
        ApplicationRuntimeOptions options,
        ILauncherLifecycle lifecycle,
        IActivationCoordinator activation,
        IHotkeyService hotkey,
        ISingleInstanceOwner singleInstance,
        IDiagnostics diagnostics,
        IIntegrationManager? integrations,
        TimeSpan integrationShutdownGrace)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(activation);
        ArgumentNullException.ThrowIfNull(hotkey);
        ArgumentNullException.ThrowIfNull(singleInstance);
        ArgumentNullException.ThrowIfNull(diagnostics);

        _options = options;
        _lifecycle = lifecycle;
        _activation = activation;
        _hotkey = hotkey;
        _singleInstance = singleInstance;
        _diagnostics = diagnostics;
        _integrations = integrations;
        _integrationShutdownGrace = integrationShutdownGrace;
    }

    /// <inheritdoc />
    public ILauncherLifecycle Lifecycle => _lifecycle;

    /// <inheritdoc />
    public async Task<ApplicationStartResult> StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _operation.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await StartCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _operation.Release();
        }
    }

    /// <inheritdoc />
    public async Task ShutdownAsync(CancellationToken cancellationToken)
    {
        await _operation.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await ShutdownCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _operation.Release();
        }
    }

    /// <inheritdoc />
    public async Task RestartAsync(CancellationToken cancellationToken)
    {
        await _operation.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await ShutdownCoreAsync(cancellationToken).ConfigureAwait(false);
            var result = await StartCoreAsync(cancellationToken).ConfigureAwait(false);
            Log(LogLevel.Information, "runtime.restarted", null, new Dictionary<string, string> { ["status"] = result.Status.ToString() });
        }
        finally
        {
            _operation.Release();
        }
    }

    private async Task<ApplicationStartResult> StartCoreAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_started is not null)
            {
                return _started;
            }
        }

        var begun = Stopwatch.GetTimestamp();
        Log(LogLevel.Information, "runtime.starting");

        var run = new CancellationTokenSource();
        lock (_gate)
        {
            _run = run;
        }

        // Subscribe before acquiring and registering, so a press or signal that arrives straight away is not lost.
        _hotkey.Pressed += OnHotkeyPressed;
        _singleInstance.ActivationRequested += OnActivationRequested;

        try
        {
            var single = _singleInstance.TryAcquire();

            if (single.Status == SingleInstanceStatus.OtherInstanceRunning)
            {
                var signaled = _singleInstance.TrySignalOwner();
                Log(LogLevel.Information, "runtime.other_instance", null, new Dictionary<string, string> { ["ownerSignaled"] = signaled ? "true" : "false" });
                Detach(run);
                return new ApplicationStartResult(ApplicationStartStatus.AnotherInstanceRunning, single, signaled, null);
            }

            if (single.Status == SingleInstanceStatus.Failed)
            {
                // Policy (ADR-0026 item 8): warn and carry on. A launcher that cannot take the lock is still usable, and a
                // second copy would find the hotkey already registered.
                Log(LogLevel.Warning, "runtime.single_instance_failed", single.ErrorCode);
            }

            var hotkey = await _hotkey.RegisterAsync(_options.Hotkey, cancellationToken).ConfigureAwait(false);
            if (hotkey.Status != HotkeyRegistrationStatus.Registered)
            {
                // Not fatal: the result carries it so the UI can show a recoverable problem, and the launcher still works.
                Log(
                    LogLevel.Warning,
                    "runtime.hotkey_unavailable",
                    hotkey.ErrorCode,
                    new Dictionary<string, string> { ["status"] = hotkey.Status.ToString() });
            }

            StartIntegrations(run.Token);

            var result = new ApplicationStartResult(ApplicationStartStatus.Started, single, false, hotkey);
            lock (_gate)
            {
                _started = result;
            }

            LifecycleLog.Write(
                _diagnostics,
                LogLevel.Information,
                Component,
                "runtime.started",
                null,
                new Dictionary<string, string>
                {
                    ["singleInstance"] = single.Status.ToString(),
                    ["hotkey"] = hotkey.Status.ToString(),
                },
                Stopwatch.GetElapsedTime(begun));
            return result;
        }
        catch
        {
            // Cancelled or failed part-way: undo what start already did, then let the caller see the exception.
            await AbortStartAsync(run).ConfigureAwait(false);
            throw;
        }
    }

    private async Task AbortStartAsync(CancellationTokenSource run)
    {
        Detach(run);

        try
        {
            await _hotkey.UnregisterAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // Cleanup is best effort and must not hide the original failure.
            Log(LogLevel.Warning, "runtime.cleanup_failed", null, LifecycleLog.ExceptionMetadata(exception));
        }

        _singleInstance.Release();

        lock (_gate)
        {
            _integrationsTask = null;
        }
    }

    private async Task ShutdownCoreAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource? run;
        Task? integrations;

        lock (_gate)
        {
            if (_started is null)
            {
                return;
            }

            run = _run;
            integrations = _integrationsTask;
        }

        Log(LogLevel.Information, "runtime.stopping");

        // Stop listening first, so a press during shutdown cannot start a new activation.
        _hotkey.Pressed -= OnHotkeyPressed;
        _singleInstance.ActivationRequested -= OnActivationRequested;

        try
        {
            await _activation.DeactivateAsync(cancellationToken).ConfigureAwait(false);
            run?.Cancel();
            await _hotkey.UnregisterAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _singleInstance.Release();
            await StopIntegrationsAsync(integrations).ConfigureAwait(false);

            lock (_gate)
            {
                _started = null;
                _run = null;
                _integrationsTask = null;
            }

            Log(LogLevel.Information, "runtime.stopped");
        }
    }

    private void StartIntegrations(CancellationToken token)
    {
        if (_integrations is null)
        {
            return;
        }

        // Task.Run, so that even an integration that blocks its calling thread cannot delay start or activation.
        var task = Task.Run(async () =>
        {
            try
            {
                await _integrations.InitializeAsync(token).ConfigureAwait(false);
                Log(LogLevel.Information, "runtime.integrations_ready");
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                Log(LogLevel.Information, "runtime.integrations_cancelled");
            }
            catch (Exception exception)
            {
                // An optional integration failing is logged by exception type only and never affects activation.
                Log(LogLevel.Warning, "runtime.integrations_failed", "runtime.integrations_failed", LifecycleLog.ExceptionMetadata(exception));
            }
        });

        lock (_gate)
        {
            _integrationsTask = task;
        }
    }

    private async Task StopIntegrationsAsync(Task? integrations)
    {
        if (integrations is null)
        {
            return;
        }

        var finished = await Task.WhenAny(integrations, Task.Delay(_integrationShutdownGrace)).ConfigureAwait(false);
        if (finished != integrations)
        {
            Log(LogLevel.Warning, "runtime.integrations_shutdown_timeout", "runtime.integrations_shutdown_timeout");
        }
    }

    private void Detach(CancellationTokenSource run)
    {
        _hotkey.Pressed -= OnHotkeyPressed;
        _singleInstance.ActivationRequested -= OnActivationRequested;

        // The source is cancelled but deliberately not disposed: a late event handler may still hold its token, and a
        // token source with no timers or registrations holds nothing that needs releasing.
        run.Cancel();
        lock (_gate)
        {
            if (ReferenceEquals(_run, run))
            {
                _run = null;
            }
        }
    }

    private void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs args)
    {
        TriggerActivation("hotkey");
    }

    private void OnActivationRequested(object? sender, EventArgs args)
    {
        TriggerActivation("second_instance");
    }

    private void TriggerActivation(string source)
    {
        CancellationToken token;

        lock (_gate)
        {
            if (_run is null)
            {
                return;
            }

            token = _run.Token;
        }

        Log(LogLevel.Information, "runtime.activation_triggered", null, new Dictionary<string, string> { ["source"] = source });

        // The event handler must return at once: the hotkey thread must not wait for an activation.
        _ = ActivateObservedAsync(token);
    }

    private async Task ActivateObservedAsync(CancellationToken token)
    {
        try
        {
            await _activation.ActivateAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Shutdown cancelled the activation; there is nothing to report.
        }
        catch (Exception exception)
        {
            // Nothing awaits this task, so an exception must be logged here or it would be lost.
            Log(LogLevel.Error, "runtime.activation_failed", "runtime.activation_failed", LifecycleLog.ExceptionMetadata(exception));
        }
    }

    private void Log(LogLevel level, string eventId, string? errorCode = null, IReadOnlyDictionary<string, string>? metadata = null)
    {
        LifecycleLog.Write(_diagnostics, level, Component, eventId, errorCode, metadata);
    }
}
