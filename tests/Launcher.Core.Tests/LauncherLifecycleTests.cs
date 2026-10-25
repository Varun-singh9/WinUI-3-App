using Launcher.Core.Diagnostics;
using Launcher.Core.Lifecycle;
using Launcher.Diagnostics;
using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Behavior of <see cref="LauncherLifecycle"/>: the Phase 1 transition table, ordered event delivery, handler
/// isolation, re-entrancy and concurrency (ADR-0023, ADR-0026).
/// </summary>
public sealed class LauncherLifecycleTests
{
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(30);

    // The legal Phase 1 transitions exactly as ADR-0026 lists them.
    private static readonly (LauncherState From, LauncherState To)[] AllowedTransitions =
    [
        (LauncherState.Hidden, LauncherState.Activating),
        (LauncherState.Hidden, LauncherState.Ready),
        (LauncherState.Activating, LauncherState.CriticalReady),
        (LauncherState.Activating, LauncherState.Closing),
        (LauncherState.Activating, LauncherState.ErrorRecovery),
        (LauncherState.CriticalReady, LauncherState.Morphing),
        (LauncherState.CriticalReady, LauncherState.Closing),
        (LauncherState.CriticalReady, LauncherState.ErrorRecovery),
        (LauncherState.Morphing, LauncherState.Ready),
        (LauncherState.Morphing, LauncherState.Closing),
        (LauncherState.Morphing, LauncherState.ErrorRecovery),
        (LauncherState.Ready, LauncherState.Closing),
        (LauncherState.Ready, LauncherState.ErrorRecovery),
        (LauncherState.Closing, LauncherState.Hidden),
        (LauncherState.ErrorRecovery, LauncherState.Hidden),
    ];

    private readonly MemoryDiagnosticsSink _sink = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private LauncherLifecycle NewLifecycle()
    {
        return new LauncherLifecycle(new DiagnosticsService([_sink]));
    }

    [Fact]
    public void A_new_lifecycle_starts_hidden()
    {
        Assert.Equal(LauncherState.Hidden, NewLifecycle().State);
    }

    [Fact]
    public void The_transition_table_matches_adr_0026_exactly()
    {
        var mismatches = new List<string>();

        foreach (var from in Enum.GetValues<LauncherState>())
        {
            foreach (var to in Enum.GetValues<LauncherState>())
            {
                var expected = AllowedTransitions.Contains((from, to));
                var actual = LifecycleTransitions.IsAllowed(from, to);
                if (expected != actual)
                {
                    mismatches.Add($"{from} -> {to}: expected {expected} but was {actual}");
                }
            }
        }

        Assert.Empty(mismatches);
        Assert.Equal(15, AllowedTransitions.Length);
    }

    [Fact]
    public void The_cold_path_walks_every_state_in_order_and_raises_matching_events()
    {
        var lifecycle = NewLifecycle();
        var events = new List<LauncherStateChangedEventArgs>();
        lifecycle.StateChanged += (_, args) => events.Add(args);

        LauncherState[] path =
        [
            LauncherState.Activating,
            LauncherState.CriticalReady,
            LauncherState.Morphing,
            LauncherState.Ready,
            LauncherState.Closing,
            LauncherState.Hidden,
        ];

        foreach (var state in path)
        {
            Assert.True(lifecycle.TryTransition(state, "test.cold"));
            Assert.Equal(state, lifecycle.State);
        }

        Assert.Equal(path.Length, events.Count);
        Assert.Equal(string.Join(",", path), string.Join(",", events.Select(e => e.Current)));
        Assert.Equal(
            "Hidden,Activating,CriticalReady,Morphing,Ready,Closing",
            string.Join(",", events.Select(e => e.Previous)));
    }

    [Fact]
    public void The_warm_path_goes_straight_from_hidden_to_ready()
    {
        var lifecycle = NewLifecycle();
        var events = new List<LauncherStateChangedEventArgs>();
        lifecycle.StateChanged += (_, args) => events.Add(args);

        Assert.True(lifecycle.TryTransition(LauncherState.Ready, "test.warm"));

        var only = Assert.Single(events);
        Assert.Equal(LauncherState.Hidden, only.Previous);
        Assert.Equal(LauncherState.Ready, only.Current);
    }

    [Fact]
    public void An_illegal_transition_is_rejected_logged_and_changes_nothing()
    {
        var lifecycle = NewLifecycle();
        var raised = 0;
        lifecycle.StateChanged += (_, _) => raised++;

        var accepted = lifecycle.TryTransition(LauncherState.Searching, "test.illegal");

        Assert.False(accepted);
        Assert.Equal(LauncherState.Hidden, lifecycle.State);
        Assert.Equal(0, raised);
        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "lifecycle.transition_rejected");
        Assert.Equal(LogLevel.Warning, logged.Level);
        Assert.Equal("Hidden", logged.Metadata["from"]);
        Assert.Equal("Searching", logged.Metadata["to"]);
        Assert.Equal("test.illegal", logged.Metadata["reason"]);
    }

    [Fact]
    public void Entering_the_current_state_again_is_rejected()
    {
        var lifecycle = NewLifecycle();
        lifecycle.TryTransition(LauncherState.Ready, "test.warm");

        Assert.False(lifecycle.TryTransition(LauncherState.Ready, "test.repeat"));
        Assert.Equal(LauncherState.Ready, lifecycle.State);
    }

    [Fact]
    public void A_blank_reason_is_a_programming_error()
    {
        var lifecycle = NewLifecycle();

        Assert.Throws<ArgumentException>(() => lifecycle.TryTransition(LauncherState.Ready, " "));
        Assert.Equal(LauncherState.Hidden, lifecycle.State);
    }

    [Fact]
    public void An_accepted_transition_is_logged_with_from_to_and_reason()
    {
        var lifecycle = NewLifecycle();

        lifecycle.TryTransition(LauncherState.Activating, "test.logged");

        var logged = Assert.Single(_sink.Entries, entry => entry.EventId == "lifecycle.state_changed");
        Assert.Equal(LogLevel.Information, logged.Level);
        Assert.Equal("Hidden", logged.Metadata["from"]);
        Assert.Equal("Activating", logged.Metadata["to"]);
        Assert.Equal("test.logged", logged.Metadata["reason"]);
    }

    [Fact]
    public void A_throwing_handler_is_logged_without_its_message_and_does_not_stop_the_others()
    {
        var lifecycle = NewLifecycle();
        var received = new List<LauncherState>();
        lifecycle.StateChanged += (_, _) => throw new InvalidOperationException("secret handler message");
        lifecycle.StateChanged += (_, args) => received.Add(args.Current);

        Assert.True(lifecycle.TryTransition(LauncherState.Ready, "test.handler"));
        Assert.True(lifecycle.TryTransition(LauncherState.Closing, "test.handler"));

        Assert.Equal("Ready,Closing", string.Join(",", received));
        var failures = _sink.Entries.Where(entry => entry.ErrorCode == "lifecycle.handler_failed").ToList();
        Assert.Equal(2, failures.Count);
        Assert.Equal("InvalidOperationException", failures[0].Metadata["exceptionType"]);
        Assert.DoesNotContain(failures[0].Metadata.Values, value => value.Contains("secret", StringComparison.Ordinal));
    }

    [Fact]
    public void A_transition_made_inside_a_handler_is_delivered_in_order_without_deadlock()
    {
        var lifecycle = NewLifecycle();
        var received = new List<LauncherState>();
        lifecycle.StateChanged += (_, args) =>
        {
            received.Add(args.Current);
            if (args.Current == LauncherState.Activating)
            {
                lifecycle.TryTransition(LauncherState.CriticalReady, "test.reentrant");
            }
        };

        Assert.True(lifecycle.TryTransition(LauncherState.Activating, "test.outer"));

        Assert.Equal("Activating,CriticalReady", string.Join(",", received));
        Assert.Equal(LauncherState.CriticalReady, lifecycle.State);
    }

    [Fact]
    public async Task Concurrent_transitions_are_serialized_and_events_arrive_in_commit_order()
    {
        var lifecycle = NewLifecycle();
        var events = new List<LauncherStateChangedEventArgs>();
        lifecycle.StateChanged += (_, args) => events.Add(args);
        var successes = 0;

        LauncherState[] cycle =
        [
            LauncherState.Ready,
            LauncherState.Closing,
            LauncherState.Hidden,
            LauncherState.Activating,
            LauncherState.CriticalReady,
            LauncherState.Morphing,
            LauncherState.Ready,
            LauncherState.Closing,
            LauncherState.Hidden,
        ];

        var workers = Enumerable.Range(0, 8).Select(_ => Task.Run(
            () =>
            {
                for (var round = 0; round < 300; round++)
                {
                    foreach (var target in cycle)
                    {
                        if (lifecycle.TryTransition(target, "test.concurrent"))
                        {
                            Interlocked.Increment(ref successes);
                        }
                    }
                }
            },
            Token));

        await Task.WhenAll(workers).WaitAsync(WaitLimit, Token);

        Assert.Equal(successes, events.Count);
        Assert.True(events.Count > 0);
        Assert.Equal(LauncherState.Hidden, events[0].Previous);
        for (var index = 0; index < events.Count; index++)
        {
            Assert.True(
                LifecycleTransitions.IsAllowed(events[index].Previous, events[index].Current),
                $"Event {index} is not a legal transition: {events[index].Previous} -> {events[index].Current}");

            if (index > 0)
            {
                Assert.Equal(events[index - 1].Current, events[index].Previous);
            }
        }

        Assert.Equal(events[^1].Current, lifecycle.State);
    }
}
