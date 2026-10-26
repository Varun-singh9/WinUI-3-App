using Launcher.Core.Diagnostics;
using Launcher.Core.Lifecycle;
using Launcher.Diagnostics;
using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Behavior of <see cref="ActivationCoordinator"/>: the cold and warm paths, repeated and concurrent requests,
/// deactivation, cancellation, and recovery from readiness failure and timeout (ADR-0026).
/// </summary>
public sealed class ActivationCoordinatorTests
{
    private const string Critical = "ui.window";

    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(30);

    private readonly MemoryDiagnosticsSink _sink = new();
    private readonly object _eventsGate = new();
    private readonly List<LauncherStateChangedEventArgs> _events = [];

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private (LauncherLifecycle Lifecycle, ReadinessCoordinator Readiness, ActivationCoordinator Coordinator) Create(TimeSpan? readinessTimeout = null)
    {
        var diagnostics = new DiagnosticsService([_sink]);
        var lifecycle = new LauncherLifecycle(diagnostics);
        lifecycle.StateChanged += (_, args) =>
        {
            lock (_eventsGate)
            {
                _events.Add(args);
            }
        };

        var readiness = new ReadinessCoordinator([Critical], readinessTimeout ?? WaitLimit, diagnostics);
        return (lifecycle, readiness, new ActivationCoordinator(lifecycle, readiness, diagnostics));
    }

    private string States()
    {
        lock (_eventsGate)
        {
            return string.Join(",", _events.Select(args => args.Current));
        }
    }

    private int EventCount()
    {
        lock (_eventsGate)
        {
            return _events.Count;
        }
    }

    private string LastRequestedKind()
    {
        return _sink.Entries.Last(entry => entry.EventId == "activation.requested").Metadata["kind"];
    }

    [Fact]
    public async Task A_cold_activation_waits_for_critical_readiness_then_reaches_ready()
    {
        var (lifecycle, readiness, coordinator) = Create();

        var activation = coordinator.ActivateAsync(Token);
        Assert.False(activation.IsCompleted);
        Assert.Equal(LauncherState.Activating, lifecycle.State);

        readiness.ReportReady(Critical);
        await activation.WaitAsync(WaitLimit, Token);

        Assert.Equal(LauncherState.Ready, lifecycle.State);
        Assert.Equal("Activating,CriticalReady,Morphing,Ready", States());
        var completed = Assert.Single(_sink.Entries, entry => entry.EventId == "activation.completed");
        Assert.Equal("Cold", completed.Metadata["kind"]);
        Assert.NotNull(completed.Duration);
    }

    [Fact]
    public async Task The_first_activation_is_cold_even_when_critical_readiness_is_already_complete()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);

        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Ready, lifecycle.State);
        Assert.Equal("Activating,CriticalReady,Morphing,Ready", States());
        Assert.Equal("Cold", LastRequestedKind());
    }

    [Fact]
    public async Task A_later_activation_is_warm_and_goes_straight_to_ready()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);
        await coordinator.ActivateAsync(Token);
        await coordinator.DeactivateAsync(Token);
        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        var before = EventCount();

        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Ready, lifecycle.State);
        lock (_eventsGate)
        {
            Assert.Equal("Ready", string.Join(",", _events.Skip(before).Select(args => args.Current)));
        }

        Assert.Equal("Warm", LastRequestedKind());
        var completed = _sink.Entries.Last(entry => entry.EventId == "activation.completed");
        Assert.Equal("Warm", completed.Metadata["kind"]);
    }

    [Fact]
    public async Task Activating_while_the_launcher_is_visible_changes_nothing()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);
        await coordinator.ActivateAsync(Token);
        var before = EventCount();

        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Ready, lifecycle.State);
        Assert.Equal(before, EventCount());
        var ignored = Assert.Single(_sink.Entries, entry => entry.EventId == "activation.ignored");
        Assert.Equal("Ready", ignored.Metadata["state"]);
    }

    [Fact]
    public async Task A_second_request_during_a_cold_activation_is_ignored()
    {
        var (lifecycle, readiness, coordinator) = Create();
        var first = coordinator.ActivateAsync(Token);

        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Activating, lifecycle.State);
        Assert.Equal("Activating", States());

        readiness.ReportReady(Critical);
        await first.WaitAsync(WaitLimit, Token);
        Assert.Equal("Activating,CriticalReady,Morphing,Ready", States());
    }

    [Fact]
    public async Task Deactivating_from_ready_closes_and_hides()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);
        await coordinator.ActivateAsync(Token);
        var before = EventCount();

        await coordinator.DeactivateAsync(Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        lock (_eventsGate)
        {
            Assert.Equal("Closing,Hidden", string.Join(",", _events.Skip(before).Select(args => args.Current)));
        }
    }

    [Fact]
    public async Task Deactivating_while_hidden_does_nothing()
    {
        var (lifecycle, _, coordinator) = Create();

        await coordinator.DeactivateAsync(Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Equal(0, EventCount());
    }

    [Fact]
    public async Task Deactivating_during_a_cold_wait_cancels_it_and_a_late_readiness_does_not_bring_it_back()
    {
        var (lifecycle, readiness, coordinator) = Create();
        var activation = coordinator.ActivateAsync(Token);
        Assert.Equal(LauncherState.Activating, lifecycle.State);

        await coordinator.DeactivateAsync(Token);
        await activation.WaitAsync(WaitLimit, Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Equal("Activating,Closing,Hidden", States());

        readiness.ReportReady(Critical);
        await Task.Delay(TimeSpan.FromMilliseconds(100), Token);

        Assert.Equal("Activating,Closing,Hidden", States());
        Assert.DoesNotContain(_sink.Entries, entry => entry.EventId == "activation.failed");
    }

    [Fact]
    public async Task Cancelling_the_caller_token_during_a_cold_wait_returns_to_hidden()
    {
        var (lifecycle, _, coordinator) = Create();
        using var cancellation = new CancellationTokenSource();
        var activation = coordinator.ActivateAsync(cancellation.Token);
        Assert.Equal(LauncherState.Activating, lifecycle.State);

        await cancellation.CancelAsync();
        await activation.WaitAsync(WaitLimit, Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Equal("Activating,Closing,Hidden", States());
        Assert.Single(_sink.Entries, entry => entry.EventId == "activation.cancelled");
    }

    [Fact]
    public async Task A_critical_failure_enters_error_recovery_then_hidden_and_the_next_activation_retries()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportFailed(Critical, "ui.window_failed");

        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Equal("Activating,ErrorRecovery,Hidden", States());
        var failed = Assert.Single(_sink.Entries, entry => entry.EventId == "activation.failed");
        Assert.Equal("activation.critical_readiness_failed", failed.ErrorCode);
        Assert.Equal(LogLevel.Error, failed.Level);
        Assert.Equal("Failed", failed.Metadata["status"]);
        Assert.Equal(Critical, failed.Metadata["capability"]);

        readiness.ReportReady(Critical);
        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Ready, lifecycle.State);
        Assert.Equal("Cold", LastRequestedKind());
    }

    [Fact]
    public async Task A_readiness_timeout_enters_error_recovery_then_hidden_and_is_logged()
    {
        var (lifecycle, readiness, coordinator) = Create(TimeSpan.FromMilliseconds(100));

        await coordinator.ActivateAsync(Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Equal("Activating,ErrorRecovery,Hidden", States());
        var failed = Assert.Single(_sink.Entries, entry => entry.EventId == "activation.failed");
        Assert.Equal("activation.critical_readiness_timed_out", failed.ErrorCode);
        Assert.Equal(LogLevel.Warning, failed.Level);
        Assert.Equal("TimedOut", failed.Metadata["status"]);

        readiness.ReportReady(Critical);
        await coordinator.ActivateAsync(Token);
        Assert.Equal(LauncherState.Ready, lifecycle.State);
    }

    [Fact]
    public async Task After_a_success_a_lost_critical_capability_makes_the_next_activation_cold_again()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);
        await coordinator.ActivateAsync(Token);
        await coordinator.DeactivateAsync(Token);
        readiness.ReportFailed(Critical, "ui.window_failed");

        await coordinator.ActivateAsync(Token);

        Assert.Equal("Cold", LastRequestedKind());
        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Single(_sink.Entries, entry => entry.EventId == "activation.failed");
    }

    [Fact]
    public async Task Concurrent_activations_run_the_cold_path_exactly_once()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);

        var activations = Enumerable.Range(0, 8).Select(_ => Task.Run(() => coordinator.ActivateAsync(Token), Token));
        await Task.WhenAll(activations).WaitAsync(WaitLimit, Token);

        Assert.Equal(LauncherState.Ready, lifecycle.State);
        Assert.Equal("Activating,CriticalReady,Morphing,Ready", States());
    }

    [Fact]
    public async Task Concurrent_activate_and_deactivate_keep_every_transition_legal_and_in_order()
    {
        var (lifecycle, readiness, coordinator) = Create();
        readiness.ReportReady(Critical);

        var workers = Enumerable.Range(0, 6).Select(_ => Task.Run(
            async () =>
            {
                for (var round = 0; round < 60; round++)
                {
                    await coordinator.ActivateAsync(Token);
                    await coordinator.DeactivateAsync(Token);
                }
            },
            Token));
        await Task.WhenAll(workers).WaitAsync(WaitLimit, Token);
        await coordinator.DeactivateAsync(Token);

        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        lock (_eventsGate)
        {
            Assert.True(_events.Count > 0);
            Assert.Equal(LauncherState.Hidden, _events[0].Previous);
            for (var index = 0; index < _events.Count; index++)
            {
                Assert.True(
                    LifecycleTransitions.IsAllowed(_events[index].Previous, _events[index].Current),
                    $"Event {index} is not legal: {_events[index].Previous} -> {_events[index].Current}");

                if (index > 0)
                {
                    Assert.Equal(_events[index - 1].Current, _events[index].Previous);
                }
            }
        }
    }

    [Fact]
    public void The_constructor_rejects_null_arguments()
    {
        var diagnostics = new DiagnosticsService([_sink]);
        var lifecycle = new LauncherLifecycle(diagnostics);
        var readiness = new ReadinessCoordinator([Critical], WaitLimit, diagnostics);

        Assert.Throws<ArgumentNullException>(() => new ActivationCoordinator(null!, readiness, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ActivationCoordinator(lifecycle, null!, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ActivationCoordinator(lifecycle, readiness, null!));
    }
}
