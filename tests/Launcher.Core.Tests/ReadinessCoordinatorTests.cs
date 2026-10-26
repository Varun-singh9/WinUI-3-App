using Launcher.Core.Diagnostics;
using Launcher.Core.Lifecycle;
using Launcher.Diagnostics;
using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Behavior of <see cref="ReadinessCoordinator"/>: the critical gate, failure, recovery, timeout, cancellation and the
/// rule that optional capabilities never gate activation (ADR-0026).
/// </summary>
public sealed class ReadinessCoordinatorTests
{
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(30);

    private readonly MemoryDiagnosticsSink _sink = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private ReadinessCoordinator New(string[] critical, TimeSpan? timeout = null)
    {
        return new ReadinessCoordinator(critical, timeout ?? WaitLimit, new DiagnosticsService([_sink]));
    }

    [Fact]
    public async Task With_no_critical_capabilities_readiness_is_complete_immediately()
    {
        var coordinator = New([]);

        Assert.True(coordinator.IsCriticalReady);
        var result = await coordinator.WaitForCriticalAsync(Token);
        Assert.Equal(ReadinessStatus.Ready, result.Status);
        Assert.Null(result.Capability);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public async Task Critical_readiness_needs_every_critical_capability()
    {
        var coordinator = New(["ui.window", "diagnostics"]);

        Assert.False(coordinator.IsCriticalReady);
        coordinator.ReportReady("ui.window");
        Assert.False(coordinator.IsCriticalReady);
        coordinator.ReportReady("diagnostics");
        Assert.True(coordinator.IsCriticalReady);

        var result = await coordinator.WaitForCriticalAsync(Token);
        Assert.Equal(ReadinessStatus.Ready, result.Status);
    }

    [Fact]
    public async Task A_wait_completes_when_the_last_critical_capability_reports_ready()
    {
        var coordinator = New(["ui.window", "diagnostics"]);
        coordinator.ReportReady("ui.window");

        var wait = coordinator.WaitForCriticalAsync(Token);
        Assert.False(wait.IsCompleted);
        coordinator.ReportReady("diagnostics");

        var result = await wait.WaitAsync(WaitLimit, Token);
        Assert.Equal(ReadinessStatus.Ready, result.Status);
    }

    [Fact]
    public async Task A_critical_failure_ends_the_wait_with_the_capability_and_code()
    {
        var coordinator = New(["ui.window", "diagnostics"]);
        var wait = coordinator.WaitForCriticalAsync(Token);

        coordinator.ReportFailed("diagnostics", "diagnostics.init_failed");

        var result = await wait.WaitAsync(WaitLimit, Token);
        Assert.Equal(ReadinessStatus.Failed, result.Status);
        Assert.Equal("diagnostics", result.Capability);
        Assert.Equal("diagnostics.init_failed", result.ErrorCode);
        Assert.False(coordinator.IsCriticalReady);
    }

    [Fact]
    public async Task A_failure_reported_before_the_wait_is_returned_immediately()
    {
        var coordinator = New(["ui.window"]);
        coordinator.ReportFailed("ui.window", "ui.window_failed");

        var result = await coordinator.WaitForCriticalAsync(Token);

        Assert.Equal(ReadinessStatus.Failed, result.Status);
        Assert.Equal("ui.window", result.Capability);
    }

    [Fact]
    public async Task A_failed_capability_can_recover_so_a_later_wait_succeeds()
    {
        var coordinator = New(["ui.window"]);
        coordinator.ReportFailed("ui.window", "ui.window_failed");
        Assert.Equal(ReadinessStatus.Failed, (await coordinator.WaitForCriticalAsync(Token)).Status);

        coordinator.ReportReady("ui.window");

        Assert.True(coordinator.IsCriticalReady);
        Assert.Equal(ReadinessStatus.Ready, (await coordinator.WaitForCriticalAsync(Token)).Status);
    }

    [Fact]
    public async Task A_wait_that_runs_out_of_time_reports_the_pending_capability_and_logs_it()
    {
        var coordinator = New(["ui.window", "diagnostics"], TimeSpan.FromMilliseconds(100));
        coordinator.ReportReady("diagnostics");

        var result = await coordinator.WaitForCriticalAsync(Token);

        Assert.Equal(ReadinessStatus.TimedOut, result.Status);
        Assert.Equal("ui.window", result.Capability);
        Assert.Null(result.ErrorCode);
        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "readiness.timed_out");
        Assert.Equal(LogLevel.Warning, logged.Level);
        Assert.Equal("TimedOut", logged.Metadata["status"]);
        Assert.NotNull(logged.Duration);
    }

    [Fact]
    public async Task Cancelling_a_wait_returns_cancelled_instead_of_throwing()
    {
        var coordinator = New(["ui.window"]);
        using var cancellation = new CancellationTokenSource();
        var wait = coordinator.WaitForCriticalAsync(cancellation.Token);

        await cancellation.CancelAsync();

        var result = await wait.WaitAsync(WaitLimit, Token);
        Assert.Equal(ReadinessStatus.Cancelled, result.Status);
        Assert.Null(result.Capability);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public async Task A_token_that_is_already_cancelled_returns_cancelled_while_readiness_is_pending()
    {
        var coordinator = New(["ui.window"]);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await coordinator.WaitForCriticalAsync(cancellation.Token);

        Assert.Equal(ReadinessStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task Optional_capabilities_never_gate_or_fail_a_wait()
    {
        var coordinator = New(["ui.window"]);
        var wait = coordinator.WaitForCriticalAsync(Token);

        coordinator.ReportFailed("provider.apps", "provider.apps_failed");
        coordinator.ReportReady("provider.files");
        Assert.False(wait.IsCompleted);
        Assert.False(coordinator.IsCriticalReady);

        coordinator.ReportReady("ui.window");

        var result = await wait.WaitAsync(WaitLimit, Token);
        Assert.Equal(ReadinessStatus.Ready, result.Status);
        Assert.True(coordinator.IsCriticalReady);
    }

    [Fact]
    public void An_unknown_capability_is_treated_as_optional_and_logged()
    {
        var coordinator = New(["ui.window"]);

        coordinator.ReportReady("something.new");
        coordinator.ReportFailed("something.else", "something.failed");

        Assert.False(coordinator.IsCriticalReady);
        var ready = Assert.Single(_sink.Entries, entry => entry.EventId == "readiness.capability_ready");
        Assert.Equal("false", ready.Metadata["critical"]);
        var failed = Assert.Single(_sink.Entries, entry => entry.EventId == "readiness.capability_failed");
        Assert.Equal("something.failed", failed.ErrorCode);
        Assert.Equal("false", failed.Metadata["critical"]);
    }

    [Fact]
    public void Blank_names_and_error_codes_are_programming_errors()
    {
        var coordinator = New(["ui.window"]);

        Assert.Throws<ArgumentException>(() => coordinator.ReportReady(" "));
        Assert.Throws<ArgumentException>(() => coordinator.ReportFailed(" ", "code"));
        Assert.Throws<ArgumentException>(() => coordinator.ReportFailed("ui.window", " "));
        Assert.False(coordinator.IsCriticalReady);
    }

    [Fact]
    public void The_constructor_rejects_bad_arguments()
    {
        var diagnostics = new DiagnosticsService([_sink]);

        Assert.Throws<ArgumentException>(() => new ReadinessCoordinator(["ui.window", " "], WaitLimit, diagnostics));
        Assert.Throws<ArgumentException>(() => new ReadinessCoordinator(["ui.window", "ui.window"], WaitLimit, diagnostics));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReadinessCoordinator(["ui.window"], TimeSpan.Zero, diagnostics));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReadinessCoordinator(["ui.window"], TimeSpan.FromSeconds(-1), diagnostics));
    }

    [Fact]
    public async Task Every_waiter_gets_the_same_result()
    {
        var coordinator = New(["ui.window"]);
        var waits = new[]
        {
            coordinator.WaitForCriticalAsync(Token),
            coordinator.WaitForCriticalAsync(Token),
            coordinator.WaitForCriticalAsync(Token),
        };

        coordinator.ReportReady("ui.window");

        var results = await Task.WhenAll(waits).WaitAsync(WaitLimit, Token);
        Assert.Equal(3, results.Length);
        Assert.True(results.All(result => result.Status == ReadinessStatus.Ready));
    }

    [Fact]
    public async Task Concurrent_reports_from_many_threads_complete_the_wait()
    {
        var names = Enumerable.Range(0, 40).Select(index => $"capability.{index}").ToArray();
        var coordinator = New(names);
        var wait = coordinator.WaitForCriticalAsync(Token);

        var reporters = names.Select(name => Task.Run(() => coordinator.ReportReady(name), Token));
        await Task.WhenAll(reporters).WaitAsync(WaitLimit, Token);

        var result = await wait.WaitAsync(WaitLimit, Token);
        Assert.Equal(ReadinessStatus.Ready, result.Status);
        Assert.True(coordinator.IsCriticalReady);
    }

    [Fact]
    public void Critical_ready_is_logged_once_even_when_reports_repeat()
    {
        var coordinator = New(["ui.window"]);

        coordinator.ReportReady("ui.window");
        coordinator.ReportReady("ui.window");

        Assert.Single(_sink.Entries, entry => entry.EventId == "readiness.critical_ready");
    }
}
