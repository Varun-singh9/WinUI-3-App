using Launcher.Core.Diagnostics;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// The semantic state machine behind <see cref="ILauncherLifecycle"/> (Implementation Specification section 4.2,
/// ADR-0023, ADR-0026). Only Core's coordinators change the state, through an internal method, so the UI and the
/// Motion System can observe the state but never set it.
/// </summary>
/// <remarks>
/// <para>
/// Transitions are serialized and checked against <see cref="LifecycleTransitions"/>. An illegal transition is rejected
/// with a Warning log and no state change; it never throws.
/// </para>
/// <para>
/// Events are raised in the order the transitions happened, outside the lock, so a handler can safely call back into
/// the lifecycle. A transition made from inside a handler, or while another thread is delivering events, is queued and
/// delivered in order by the thread that is already delivering, so an event can arrive on a different thread from the
/// one that caused it. Handlers should read <see cref="LauncherStateChangedEventArgs.Current"/> rather than
/// <see cref="State"/>, because <see cref="State"/> may already be ahead of the event being delivered. A handler that
/// throws is logged by exception type only and the other handlers still run.
/// </para>
/// </remarks>
public sealed class LauncherLifecycle : ILauncherLifecycle
{
    private const string Component = "Lifecycle";

    private readonly IDiagnostics _diagnostics;
    private readonly Lock _gate = new();
    private readonly Queue<PendingChange> _pending = new();
    private LauncherState _state = LauncherState.Hidden;
    private bool _dispatching;

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherLifecycle"/> class in the <see cref="LauncherState.Hidden"/> state.
    /// </summary>
    /// <param name="diagnostics">Where transitions and failures are written.</param>
    public LauncherLifecycle(IDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        _diagnostics = diagnostics;
    }

    /// <inheritdoc />
    public event EventHandler<LauncherStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public LauncherState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Tries to move to a new state. Only Core's coordinators call this.
    /// </summary>
    /// <param name="target">The state to enter.</param>
    /// <param name="reason">A short, stable, non-sensitive reason such as <c>activation.cold</c>, written to diagnostics.</param>
    /// <returns><see langword="true"/> when the state changed; <see langword="false"/> when the transition is not allowed.</returns>
    internal bool TryTransition(LauncherState target, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        LauncherState from;
        var accepted = false;
        var startDelivery = false;

        lock (_gate)
        {
            from = _state;
            if (LifecycleTransitions.IsAllowed(from, target))
            {
                accepted = true;
                _state = target;
                _pending.Enqueue(new PendingChange(new LauncherStateChangedEventArgs(from, target), reason));

                // Whoever finds nobody delivering becomes the deliverer.
                startDelivery = !_dispatching;
                _dispatching = true;
            }
        }

        if (!accepted)
        {
            LifecycleLog.Write(
                _diagnostics,
                LogLevel.Warning,
                Component,
                "lifecycle.transition_rejected",
                "lifecycle.transition_rejected",
                new Dictionary<string, string>
                {
                    ["from"] = from.ToString(),
                    ["to"] = target.ToString(),
                    ["reason"] = reason,
                });
            return false;
        }

        if (startDelivery)
        {
            Deliver();
        }

        return true;
    }

    private void Deliver()
    {
        while (true)
        {
            PendingChange change;

            lock (_gate)
            {
                if (!_pending.TryDequeue(out change))
                {
                    _dispatching = false;
                    return;
                }
            }

            Raise(change);
        }
    }

    private void Raise(PendingChange change)
    {
        var args = change.Args;

        LifecycleLog.Write(
            _diagnostics,
            LogLevel.Information,
            Component,
            "lifecycle.state_changed",
            null,
            new Dictionary<string, string>
            {
                ["from"] = args.Previous.ToString(),
                ["to"] = args.Current.ToString(),
                ["reason"] = change.Reason,
            });

        var handlers = StateChanged;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<LauncherStateChangedEventArgs>>())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception exception)
            {
                // A handler is foreign code. One that throws must not stop delivery to the others or end the process.
                // Only the exception type is logged, never its message.
                LifecycleLog.Write(
                    _diagnostics,
                    LogLevel.Error,
                    Component,
                    "lifecycle.handler_failed",
                    "lifecycle.handler_failed",
                    LifecycleLog.ExceptionMetadata(exception));
            }
        }
    }

    private readonly record struct PendingChange(LauncherStateChangedEventArgs Args, string Reason);
}
