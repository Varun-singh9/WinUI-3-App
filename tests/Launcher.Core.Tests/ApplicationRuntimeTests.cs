using Launcher.Core.Diagnostics;
using Launcher.Core.Integrations;
using Launcher.Core.Lifecycle;
using Launcher.Diagnostics;
using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Behavior of <see cref="ApplicationRuntime"/>: start order, a second instance, hotkey and single-instance failures,
/// activation triggers, optional integrations, shutdown, restart and cancellation (ADR-0026). The hotkey and
/// single-instance pieces are fakes; the lifecycle, readiness and activation coordinators are the real ones.
/// </summary>
public sealed class ApplicationRuntimeTests
{
    private const string Critical = "ui.window";

    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(30);
    private static readonly HotkeyGesture Gesture = new(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, 0x87);

    private readonly MemoryDiagnosticsSink _sink = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Rig CreateRig(FakeIntegrationManager? integrations = null, bool criticalReady = true, TimeSpan? grace = null)
    {
        return new Rig(_sink, integrations, criticalReady, grace ?? TimeSpan.FromSeconds(2));
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + WaitLimit;
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "The condition was not met in time.");
            await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Start_subscribes_first_then_acquires_ownership_then_registers_the_options_hotkey()
    {
        var rig = CreateRig();

        await rig.Runtime.StartAsync(Token);

        Assert.True(rig.Single.SubscribedBeforeAcquire);
        Assert.Equal("single.acquire, hotkey.register", rig.Calls.Joined);
        Assert.Equal(Gesture, rig.Hotkey.RequestedGesture);
    }

    [Fact]
    public async Task A_normal_start_reports_started_and_leaves_the_launcher_hidden()
    {
        var rig = CreateRig();

        var result = await rig.Runtime.StartAsync(Token);

        Assert.Equal(ApplicationStartStatus.Started, result.Status);
        Assert.Equal(SingleInstanceStatus.Owner, result.SingleInstance.Status);
        Assert.False(result.OwnerSignaled);
        Assert.NotNull(result.Hotkey);
        Assert.Equal(HotkeyRegistrationStatus.Registered, result.Hotkey.Status);
        Assert.Equal(LauncherState.Hidden, rig.Runtime.Lifecycle.State);
        var started = Assert.Single(_sink.Entries, entry => entry.EventId == "runtime.started");
        Assert.NotNull(started.Duration);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task When_another_instance_owns_it_the_owner_is_signaled_and_nothing_else_starts(bool ownerSignaled)
    {
        var integrations = new FakeIntegrationManager();
        var rig = CreateRig(integrations);
        rig.Single.NextAcquire = new SingleInstanceResult(SingleInstanceStatus.OtherInstanceRunning, null);
        rig.Single.SignalResult = ownerSignaled;

        var result = await rig.Runtime.StartAsync(Token);

        Assert.Equal(ApplicationStartStatus.AnotherInstanceRunning, result.Status);
        Assert.Equal(SingleInstanceStatus.OtherInstanceRunning, result.SingleInstance.Status);
        Assert.Equal(ownerSignaled, result.OwnerSignaled);
        Assert.Null(result.Hotkey);
        Assert.Equal("single.acquire, single.signal", rig.Calls.Joined);
        Assert.False(rig.Hotkey.HasSubscribers);
        Assert.False(rig.Single.HasSubscribers);
        Assert.Equal(0, integrations.Started);
    }

    [Fact]
    public async Task A_single_instance_failure_is_logged_and_the_launcher_still_starts()
    {
        var rig = CreateRig();
        rig.Single.NextAcquire = new SingleInstanceResult(SingleInstanceStatus.Failed, "single_instance.access_denied");

        var result = await rig.Runtime.StartAsync(Token);

        Assert.Equal(ApplicationStartStatus.Started, result.Status);
        Assert.Equal(SingleInstanceStatus.Failed, result.SingleInstance.Status);
        Assert.Equal("single.acquire, hotkey.register", rig.Calls.Joined);
        var logged = Assert.Single(_sink.Entries, entry => entry.EventId == "runtime.single_instance_failed");
        Assert.Equal("single_instance.access_denied", logged.ErrorCode);
        Assert.Equal(LogLevel.Warning, logged.Level);
    }

    [Fact]
    public async Task A_hotkey_failure_is_reported_without_an_error_state_and_the_launcher_still_activates()
    {
        var rig = CreateRig();
        rig.Hotkey.NextResult = new HotkeyRegistrationResult(HotkeyRegistrationStatus.InUse, Gesture, "hotkey.in_use");

        var result = await rig.Runtime.StartAsync(Token);

        Assert.Equal(ApplicationStartStatus.Started, result.Status);
        Assert.NotNull(result.Hotkey);
        Assert.Equal(HotkeyRegistrationStatus.InUse, result.Hotkey.Status);
        Assert.Equal(LauncherState.Hidden, rig.Runtime.Lifecycle.State);
        var logged = Assert.Single(_sink.Entries, entry => entry.EventId == "runtime.hotkey_unavailable");
        Assert.Equal("hotkey.in_use", logged.ErrorCode);
        Assert.Equal("InUse", logged.Metadata["status"]);

        rig.Single.RaiseActivationRequested();
        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);
    }

    [Fact]
    public async Task A_hotkey_press_activates_the_launcher_through_the_cold_path()
    {
        var rig = CreateRig();
        await rig.Runtime.StartAsync(Token);

        rig.Hotkey.RaisePressed();

        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);
        Assert.Equal("Activating,CriticalReady,Morphing,Ready", rig.States());
        var triggered = Assert.Single(_sink.Entries, entry => entry.EventId == "runtime.activation_triggered");
        Assert.Equal("hotkey", triggered.Metadata["source"]);
    }

    [Fact]
    public async Task A_second_instance_signal_activates_the_launcher()
    {
        var rig = CreateRig();
        await rig.Runtime.StartAsync(Token);

        rig.Single.RaiseActivationRequested();

        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);
        var triggered = Assert.Single(_sink.Entries, entry => entry.EventId == "runtime.activation_triggered");
        Assert.Equal("second_instance", triggered.Metadata["source"]);
    }

    [Fact]
    public async Task Starting_twice_returns_the_same_result_and_starts_once()
    {
        var rig = CreateRig();

        var first = await rig.Runtime.StartAsync(Token);
        var second = await rig.Runtime.StartAsync(Token);

        Assert.Equal(first, second);
        Assert.Equal("single.acquire, hotkey.register", rig.Calls.Joined);
    }

    [Fact]
    public async Task A_pending_integration_never_blocks_start_or_activation_and_is_cancelled_on_shutdown()
    {
        var integrations = new FakeIntegrationManager();
        var rig = CreateRig(integrations);

        var result = await rig.Runtime.StartAsync(Token);
        await WaitUntilAsync(() => integrations.Started == 1);
        rig.Hotkey.RaisePressed();
        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);

        Assert.Equal(ApplicationStartStatus.Started, result.Status);
        Assert.False(integrations.Gate.Task.IsCompleted);

        await rig.Runtime.ShutdownAsync(Token);

        Assert.True(integrations.ObservedCancellation);
        Assert.Equal(LauncherState.Hidden, rig.Runtime.Lifecycle.State);
    }

    [Fact]
    public async Task A_failing_integration_is_logged_by_type_and_does_not_affect_activation()
    {
        var integrations = new FakeIntegrationManager { Throw = new InvalidOperationException("secret integration message") };
        var rig = CreateRig(integrations);

        var result = await rig.Runtime.StartAsync(Token);
        await WaitUntilAsync(() => _sink.Entries.Any(entry => entry.ErrorCode == "runtime.integrations_failed"));
        rig.Hotkey.RaisePressed();
        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);

        Assert.Equal(ApplicationStartStatus.Started, result.Status);
        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "runtime.integrations_failed");
        Assert.Equal("InvalidOperationException", logged.Metadata["exceptionType"]);
        Assert.DoesNotContain(logged.Metadata.Values, value => value.Contains("secret", StringComparison.Ordinal));
    }

    [Fact]
    public async Task An_integration_that_blocks_and_ignores_cancellation_cannot_delay_start_or_hang_shutdown()
    {
        var integrations = new FakeIntegrationManager { BlockSynchronously = true, IgnoreCancellation = true };
        var rig = CreateRig(integrations, grace: TimeSpan.FromMilliseconds(100));

        try
        {
            var result = await rig.Runtime.StartAsync(Token);
            await WaitUntilAsync(() => integrations.Started == 1);
            Assert.Equal(ApplicationStartStatus.Started, result.Status);

            await rig.Runtime.ShutdownAsync(Token);

            var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "runtime.integrations_shutdown_timeout");
            Assert.Equal(LogLevel.Warning, logged.Level);
        }
        finally
        {
            integrations.Gate.TrySetResult();
        }
    }

    [Fact]
    public async Task Shutdown_from_ready_closes_stops_listening_and_releases_in_order_and_is_idempotent()
    {
        var rig = CreateRig();
        await rig.Runtime.StartAsync(Token);
        rig.Hotkey.RaisePressed();
        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);

        await rig.Runtime.ShutdownAsync(Token);

        Assert.Equal(LauncherState.Hidden, rig.Runtime.Lifecycle.State);
        Assert.EndsWith("Closing,Hidden", rig.States(), StringComparison.Ordinal);
        Assert.Equal("single.acquire, hotkey.register, hotkey.unregister, single.release", rig.Calls.Joined);
        Assert.False(rig.Hotkey.HasSubscribers);
        Assert.False(rig.Single.HasSubscribers);

        await rig.Runtime.ShutdownAsync(Token);

        Assert.Equal("single.acquire, hotkey.register, hotkey.unregister, single.release", rig.Calls.Joined);
    }

    [Fact]
    public async Task A_press_after_shutdown_does_nothing()
    {
        var rig = CreateRig();
        await rig.Runtime.StartAsync(Token);
        await rig.Runtime.ShutdownAsync(Token);

        rig.Hotkey.RaisePressed();
        rig.Single.RaiseActivationRequested();
        await Task.Delay(TimeSpan.FromMilliseconds(100), Token);

        Assert.Equal(LauncherState.Hidden, rig.Runtime.Lifecycle.State);
        Assert.Equal(string.Empty, rig.States());
    }

    [Fact]
    public async Task Shutdown_during_a_cold_activation_cancels_it_and_returns_to_hidden_without_an_error()
    {
        var rig = CreateRig(criticalReady: false);
        await rig.Runtime.StartAsync(Token);
        rig.Hotkey.RaisePressed();
        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Activating);

        await rig.Runtime.ShutdownAsync(Token);

        Assert.Equal(LauncherState.Hidden, rig.Runtime.Lifecycle.State);
        Assert.Equal("Activating,Closing,Hidden", rig.States());
        Assert.DoesNotContain(_sink.Entries, entry => entry.EventId == "activation.failed");
    }

    [Fact]
    public async Task Restart_shuts_down_and_starts_again_and_the_hotkey_works_afterwards()
    {
        var rig = CreateRig();
        await rig.Runtime.StartAsync(Token);

        await rig.Runtime.RestartAsync(Token);

        Assert.Equal(
            "single.acquire, hotkey.register, hotkey.unregister, single.release, single.acquire, hotkey.register",
            rig.Calls.Joined);
        Assert.True(rig.Hotkey.HasSubscribers);

        rig.Hotkey.RaisePressed();
        await WaitUntilAsync(() => rig.Runtime.Lifecycle.State == LauncherState.Ready);
    }

    [Fact]
    public async Task Cancelling_start_part_way_cleans_up_and_a_later_start_works()
    {
        var rig = CreateRig();
        using var cancellation = new CancellationTokenSource();
        rig.Hotkey.OnRegister = cancellation.Cancel;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => rig.Runtime.StartAsync(cancellation.Token));

        Assert.Equal("single.acquire, hotkey.register, hotkey.unregister, single.release", rig.Calls.Joined);
        Assert.False(rig.Hotkey.HasSubscribers);
        Assert.False(rig.Single.HasSubscribers);

        rig.Hotkey.OnRegister = null;
        var result = await rig.Runtime.StartAsync(Token);
        Assert.Equal(ApplicationStartStatus.Started, result.Status);
    }

    [Fact]
    public async Task An_unexpected_failure_during_start_cleans_up_and_is_rethrown()
    {
        var rig = CreateRig();
        rig.Hotkey.ThrowOnRegister = new InvalidOperationException("unexpected");

        await Assert.ThrowsAsync<InvalidOperationException>(() => rig.Runtime.StartAsync(Token));

        Assert.Equal("single.acquire, hotkey.register, hotkey.unregister, single.release", rig.Calls.Joined);
        Assert.False(rig.Hotkey.HasSubscribers);
        Assert.False(rig.Single.HasSubscribers);
    }

    [Fact]
    public async Task A_token_that_is_already_cancelled_stops_start_before_it_does_anything()
    {
        var rig = CreateRig();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => rig.Runtime.StartAsync(cancellation.Token));

        Assert.Equal(string.Empty, rig.Calls.Joined);
    }

    [Fact]
    public void The_constructor_rejects_null_arguments()
    {
        var rig = CreateRig();
        var options = new ApplicationRuntimeOptions(Gesture, TimeSpan.FromSeconds(5));
        var diagnostics = new DiagnosticsService([_sink]);

        Assert.Throws<ArgumentNullException>(() => new ApplicationRuntime(null!, rig.Lifecycle, rig.Activation, rig.Hotkey, rig.Single, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ApplicationRuntime(options, null!, rig.Activation, rig.Hotkey, rig.Single, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ApplicationRuntime(options, rig.Lifecycle, null!, rig.Hotkey, rig.Single, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ApplicationRuntime(options, rig.Lifecycle, rig.Activation, null!, rig.Single, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ApplicationRuntime(options, rig.Lifecycle, rig.Activation, rig.Hotkey, null!, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new ApplicationRuntime(options, rig.Lifecycle, rig.Activation, rig.Hotkey, rig.Single, null!));
    }

    /// <summary>Everything one runtime test needs, wired the way the composition root will wire it.</summary>
    private sealed class Rig
    {
        private readonly object _eventsGate = new();
        private readonly List<LauncherState> _states = [];

        public Rig(MemoryDiagnosticsSink sink, FakeIntegrationManager? integrations, bool criticalReady, TimeSpan grace)
        {
            var diagnostics = new DiagnosticsService([sink]);
            Calls = new CallLog();
            Hotkey = new FakeHotkeyService(Calls);
            Single = new FakeSingleInstanceOwner(Calls, Hotkey);
            Lifecycle = new LauncherLifecycle(diagnostics);
            Lifecycle.StateChanged += (_, args) =>
            {
                lock (_eventsGate)
                {
                    _states.Add(args.Current);
                }
            };

            var readiness = new ReadinessCoordinator([Critical], WaitLimit, diagnostics);
            if (criticalReady)
            {
                readiness.ReportReady(Critical);
            }

            Activation = new ActivationCoordinator(Lifecycle, readiness, diagnostics);
            var options = new ApplicationRuntimeOptions(Gesture, ApplicationRuntimeOptions.DefaultCriticalReadinessTimeout);
            Runtime = new ApplicationRuntime(options, Lifecycle, Activation, Hotkey, Single, diagnostics, integrations, grace);
        }

        public CallLog Calls { get; }

        public FakeHotkeyService Hotkey { get; }

        public FakeSingleInstanceOwner Single { get; }

        public LauncherLifecycle Lifecycle { get; }

        public ActivationCoordinator Activation { get; }

        public ApplicationRuntime Runtime { get; }

        public string States()
        {
            lock (_eventsGate)
            {
                return string.Join(",", _states);
            }
        }
    }
}
