using System.Diagnostics;
using Launcher.Core.Diagnostics;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// The readiness gate behind <see cref="IReadinessCoordinator"/> (roadmap Phase 1, ADR-0026). The composition root
/// names the critical capabilities when it creates the coordinator. Components then report capabilities ready or
/// failed by name, and activation waits for the critical ones only.
/// </summary>
/// <remarks>
/// <para>
/// A capability whose name is not in the critical set is optional: reports about it are logged but never change a
/// wait, so an optional provider can neither block nor fail activation.
/// </para>
/// <para>
/// A wait ends with the first of: every critical capability ready, a critical capability failed, the timeout elapsing,
/// or cancellation. The timeout is measured from the start of each wait. All outcomes are returned as a
/// <see cref="ReadinessResult"/>; nothing is thrown for them.
/// </para>
/// </remarks>
public sealed class ReadinessCoordinator : IReadinessCoordinator
{
    private const string Component = "Readiness";
    private const string TimedOutCode = "readiness.timed_out";

    private readonly IDiagnostics _diagnostics;
    private readonly TimeSpan _timeout;
    private readonly string[] _critical;
    private readonly HashSet<string> _criticalSet = new(StringComparer.Ordinal);
    private readonly HashSet<string> _ready = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _failures = new(StringComparer.Ordinal);
    private readonly List<TaskCompletionSource<ReadinessResult>> _waiters = [];
    private readonly Lock _gate = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadinessCoordinator"/> class.
    /// </summary>
    /// <param name="criticalCapabilities">The names of the critical capabilities. May be empty, in which case critical readiness is complete immediately.</param>
    /// <param name="criticalReadinessTimeout">How long one wait lasts before it ends as <see cref="ReadinessStatus.TimedOut"/>. Must be positive.</param>
    /// <param name="diagnostics">Where reports and wait outcomes are written.</param>
    /// <exception cref="ArgumentException">A critical name is blank or listed twice.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timeout is not positive.</exception>
    public ReadinessCoordinator(IReadOnlyCollection<string> criticalCapabilities, TimeSpan criticalReadinessTimeout, IDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(criticalCapabilities);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(criticalReadinessTimeout, TimeSpan.Zero);

        foreach (var name in criticalCapabilities)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(criticalCapabilities));
            if (!_criticalSet.Add(name))
            {
                throw new ArgumentException($"The critical capability '{name}' is listed more than once.", nameof(criticalCapabilities));
            }
        }

        _critical = [.. criticalCapabilities];
        _timeout = criticalReadinessTimeout;
        _diagnostics = diagnostics;
    }

    /// <inheritdoc />
    public bool IsCriticalReady
    {
        get
        {
            lock (_gate)
            {
                return EvaluateLocked()?.Status == ReadinessStatus.Ready;
            }
        }
    }

    /// <inheritdoc />
    public void ReportReady(string capability)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);

        var critical = _criticalSet.Contains(capability);
        var becameReady = false;
        ReadinessResult? completed = null;
        TaskCompletionSource<ReadinessResult>[] waiters = [];

        if (critical)
        {
            lock (_gate)
            {
                var wasReady = EvaluateLocked()?.Status == ReadinessStatus.Ready;
                _failures.Remove(capability);
                _ready.Add(capability);
                completed = EvaluateLocked();
                becameReady = !wasReady && completed?.Status == ReadinessStatus.Ready;
                waiters = completed is null ? [] : [.. _waiters];
            }
        }

        LifecycleLog.Write(
            _diagnostics,
            LogLevel.Information,
            Component,
            "readiness.capability_ready",
            null,
            new Dictionary<string, string> { ["capability"] = capability, ["critical"] = critical ? "true" : "false" });

        if (becameReady)
        {
            LifecycleLog.Write(_diagnostics, LogLevel.Information, Component, "readiness.critical_ready");
        }

        Complete(waiters, completed);
    }

    /// <inheritdoc />
    public void ReportFailed(string capability, string errorCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        var critical = _criticalSet.Contains(capability);
        ReadinessResult? completed = null;
        TaskCompletionSource<ReadinessResult>[] waiters = [];

        if (critical)
        {
            lock (_gate)
            {
                _ready.Remove(capability);
                _failures[capability] = errorCode;
                completed = EvaluateLocked();
                waiters = completed is null ? [] : [.. _waiters];
            }
        }

        LifecycleLog.Write(
            _diagnostics,
            LogLevel.Warning,
            Component,
            "readiness.capability_failed",
            errorCode,
            new Dictionary<string, string> { ["capability"] = capability, ["critical"] = critical ? "true" : "false" });

        Complete(waiters, completed);
    }

    /// <inheritdoc />
    public async Task<ReadinessResult> WaitForCriticalAsync(CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var waiter = new TaskCompletionSource<ReadinessResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var alreadyCancelled = false;

        lock (_gate)
        {
            var current = EvaluateLocked();
            if (current is not null)
            {
                return current;
            }

            alreadyCancelled = cancellationToken.IsCancellationRequested;
            if (!alreadyCancelled)
            {
                _waiters.Add(waiter);
            }
        }

        if (alreadyCancelled)
        {
            return Finish(new ReadinessResult(ReadinessStatus.Cancelled, null, null), started);
        }

        try
        {
            return Finish(await waiter.Task.WaitAsync(_timeout, cancellationToken).ConfigureAwait(false), started);
        }
        catch (TimeoutException)
        {
            return Finish(waiter.Task.IsCompletedSuccessfully ? waiter.Task.Result : TimedOut(), started);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Finish(waiter.Task.IsCompletedSuccessfully ? waiter.Task.Result : new ReadinessResult(ReadinessStatus.Cancelled, null, null), started);
        }
        finally
        {
            lock (_gate)
            {
                _waiters.Remove(waiter);
            }
        }
    }

    private static void Complete(TaskCompletionSource<ReadinessResult>[] waiters, ReadinessResult? result)
    {
        if (result is null)
        {
            return;
        }

        foreach (var waiter in waiters)
        {
            waiter.TrySetResult(result);
        }
    }

    private ReadinessResult? EvaluateLocked()
    {
        foreach (var name in _critical)
        {
            if (_failures.TryGetValue(name, out var errorCode))
            {
                return new ReadinessResult(ReadinessStatus.Failed, name, errorCode);
            }
        }

        return _critical.All(_ready.Contains) ? new ReadinessResult(ReadinessStatus.Ready, null, null) : null;
    }

    private ReadinessResult TimedOut()
    {
        string? pending;
        lock (_gate)
        {
            pending = _critical.FirstOrDefault(name => !_ready.Contains(name));
        }

        return new ReadinessResult(ReadinessStatus.TimedOut, pending, null);
    }

    private ReadinessResult Finish(ReadinessResult result, long started)
    {
        var duration = Stopwatch.GetElapsedTime(started);
        var metadata = new Dictionary<string, string> { ["status"] = result.Status.ToString() };
        if (result.Capability is not null)
        {
            metadata["capability"] = result.Capability;
        }

        var (level, errorCode) = result.Status switch
        {
            ReadinessStatus.Failed => (LogLevel.Warning, result.ErrorCode),
            ReadinessStatus.TimedOut => (LogLevel.Warning, TimedOutCode),
            _ => (LogLevel.Information, (string?)null),
        };

        LifecycleLog.Write(_diagnostics, level, Component, "readiness.wait_completed", errorCode, metadata, duration);
        return result;
    }
}
