using System.Diagnostics;
using Launcher.Core.Diagnostics;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// Coordinates cold and warm activation behind <see cref="IActivationCoordinator"/> (Implementation Specification
/// section 7, ADR-0026). It drives the <see cref="LauncherLifecycle"/> through the cold or warm path and never touches
/// UI, animation or providers.
/// </summary>
/// <remarks>
/// <para>
/// Cold means the first activation of the process, or any activation while critical readiness is not complete. The cold
/// path is Hidden, Activating, CriticalReady, Morphing, Ready. The warm path is Hidden to Ready. A request while the
/// launcher is already visible, or in the middle of a transition, changes nothing.
/// </para>
/// <para>
/// Presentation timing is provisional (ADR-0026 item 7): once critical readiness is reached, Morphing and Ready follow
/// immediately and Closing is followed by Hidden immediately. Core does not wait for animation, so input is never
/// blocked.
/// </para>
/// <para>
/// Failures are recoverable: a critical readiness failure or timeout enters ErrorRecovery and then Hidden, and the next
/// activation tries again as cold. Cancelling an activation, or deactivating while it waits, returns to Hidden through
/// Closing; the activation task then completes normally.
/// </para>
/// </remarks>
public sealed class ActivationCoordinator : IActivationCoordinator
{
    private const string Component = "Activation";

    private readonly LauncherLifecycle _lifecycle;
    private readonly IReadinessCoordinator _readiness;
    private readonly IDiagnostics _diagnostics;
    private readonly Lock _gate = new();
    private ActivationRun? _run;
    private bool _firstActivationCompleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivationCoordinator"/> class.
    /// </summary>
    /// <param name="lifecycle">The state machine this coordinator drives.</param>
    /// <param name="readiness">The readiness gate that decides when a cold activation can proceed.</param>
    /// <param name="diagnostics">Where activations and failures are written.</param>
    public ActivationCoordinator(LauncherLifecycle lifecycle, IReadinessCoordinator readiness, IDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(readiness);
        ArgumentNullException.ThrowIfNull(diagnostics);

        _lifecycle = lifecycle;
        _readiness = readiness;
        _diagnostics = diagnostics;
    }

    /// <inheritdoc />
    public async Task ActivateAsync(CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();

        var state = _lifecycle.State;
        if (state != LauncherState.Hidden)
        {
            Log(LogLevel.Information, "activation.ignored", null, new Dictionary<string, string> { ["state"] = state.ToString() });
            return;
        }

        bool firstActivationCompleted;
        lock (_gate)
        {
            firstActivationCompleted = _firstActivationCompleted;
        }

        var kind = firstActivationCompleted && _readiness.IsCriticalReady ? ActivationKind.Warm : ActivationKind.Cold;
        Log(LogLevel.Information, "activation.requested", null, KindMetadata(kind));

        if (kind == ActivationKind.Warm)
        {
            if (_lifecycle.TryTransition(LauncherState.Ready, "activation.warm"))
            {
                LogCompleted(kind, started);
            }
            else
            {
                LogIgnoredByRace();
            }

            return;
        }

        var run = new ActivationRun(cancellationToken);
        bool entered;
        lock (_gate)
        {
            _run = run;
            entered = _lifecycle.TryTransition(LauncherState.Activating, "activation.cold");
            if (!entered)
            {
                _run = null;
            }
        }

        if (!entered)
        {
            run.Dispose();
            LogIgnoredByRace();
            return;
        }

        try
        {
            var result = await _readiness.WaitForCriticalAsync(run.Token).ConfigureAwait(false);
            HandleReadiness(result, run, started);
        }
        finally
        {
            lock (_gate)
            {
                if (ReferenceEquals(_run, run))
                {
                    _run = null;
                }
            }

            run.Dispose();
        }
    }

    /// <summary>
    /// Deactivates the launcher. Deactivation is immediate and cannot be cancelled, so the token is not used; a launcher
    /// that is shutting down must always be able to close.
    /// </summary>
    /// <param name="cancellationToken">Not used.</param>
    /// <returns>A task that is already complete: the launcher is hidden, or is no longer in a state that can close.</returns>
    public Task DeactivateAsync(CancellationToken cancellationToken)
    {
        var state = _lifecycle.State;

        switch (state)
        {
            case LauncherState.Activating:
            case LauncherState.CriticalReady:
            case LauncherState.Morphing:
                ActivationRun? run;
                lock (_gate)
                {
                    run = _run;
                }

                // Stop the wait first. Marking the run tells the activation that this method does the closing.
                run?.CancelForDeactivate();
                Close("deactivate.cancelled_activation", state);
                break;

            case LauncherState.Ready:
                Close("deactivate", state);
                break;

            default:
                // Hidden: nothing to do. Closing and ErrorRecovery are already on their way to Hidden. States that
                // arrive in later phases extend the transition table and this method with their own records.
                break;
        }

        return Task.CompletedTask;
    }

    private static Dictionary<string, string> KindMetadata(ActivationKind kind)
    {
        return new Dictionary<string, string> { ["kind"] = kind.ToString() };
    }

    private void HandleReadiness(ReadinessResult result, ActivationRun run, long started)
    {
        switch (result.Status)
        {
            case ReadinessStatus.Ready:
                // Provisional presentation timing: Morphing and Ready follow at once (ADR-0026 item 7).
                if (_lifecycle.TryTransition(LauncherState.CriticalReady, "activation.critical_ready")
                    && _lifecycle.TryTransition(LauncherState.Morphing, "activation.morph")
                    && _lifecycle.TryTransition(LauncherState.Ready, "activation.ready"))
                {
                    lock (_gate)
                    {
                        _firstActivationCompleted = true;
                    }

                    LogCompleted(ActivationKind.Cold, started);
                }
                else
                {
                    Log(LogLevel.Information, "activation.aborted", null, KindMetadata(ActivationKind.Cold));
                }

                break;

            case ReadinessStatus.Failed:
            case ReadinessStatus.TimedOut:
                Fail(result, started);
                break;

            default:
                if (!run.CancelledByDeactivate)
                {
                    // The caller cancelled. Nobody else is closing, so close here.
                    Log(LogLevel.Information, "activation.cancelled", null, KindMetadata(ActivationKind.Cold));
                    Close("activation.cancelled", _lifecycle.State);
                }

                break;
        }
    }

    private void Fail(ReadinessResult result, long started)
    {
        var timedOut = result.Status == ReadinessStatus.TimedOut;
        var errorCode = timedOut ? "activation.critical_readiness_timed_out" : "activation.critical_readiness_failed";

        if (!_lifecycle.TryTransition(LauncherState.ErrorRecovery, errorCode))
        {
            // Deactivated in the meantime; the launcher is already closing.
            Log(LogLevel.Information, "activation.aborted", null, KindMetadata(ActivationKind.Cold));
            return;
        }

        var metadata = KindMetadata(ActivationKind.Cold);
        metadata["status"] = result.Status.ToString();
        if (result.Capability is not null)
        {
            metadata["capability"] = result.Capability;
        }

        LifecycleLog.Write(
            _diagnostics,
            timedOut ? LogLevel.Warning : LogLevel.Error,
            Component,
            "activation.failed",
            errorCode,
            metadata,
            Stopwatch.GetElapsedTime(started));

        // The failure is recoverable: go back to Hidden, and the next activation retries as cold.
        _lifecycle.TryTransition(LauncherState.Hidden, "activation.recovered");
    }

    private void Close(string reason, LauncherState from)
    {
        if (_lifecycle.TryTransition(LauncherState.Closing, reason))
        {
            _lifecycle.TryTransition(LauncherState.Hidden, reason + ".hidden");
            Log(LogLevel.Information, "activation.deactivated", null, new Dictionary<string, string> { ["from"] = from.ToString() });
        }
    }

    private void LogCompleted(ActivationKind kind, long started)
    {
        LifecycleLog.Write(
            _diagnostics,
            LogLevel.Information,
            Component,
            "activation.completed",
            null,
            KindMetadata(kind),
            Stopwatch.GetElapsedTime(started));
    }

    private void LogIgnoredByRace()
    {
        Log(LogLevel.Information, "activation.ignored", null, new Dictionary<string, string> { ["state"] = _lifecycle.State.ToString() });
    }

    private void Log(LogLevel level, string eventId, string? errorCode, IReadOnlyDictionary<string, string>? metadata)
    {
        LifecycleLog.Write(_diagnostics, level, Component, eventId, errorCode, metadata);
    }

    private sealed class ActivationRun(CancellationToken external) : IDisposable
    {
        private readonly CancellationTokenSource _source = CancellationTokenSource.CreateLinkedTokenSource(external);
        private volatile bool _cancelledByDeactivate;

        public CancellationToken Token => _source.Token;

        public bool CancelledByDeactivate => _cancelledByDeactivate;

        public void CancelForDeactivate()
        {
            _cancelledByDeactivate = true;

            try
            {
                _source.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The activation already finished and released its token source; there is nothing left to cancel.
            }
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }
}
