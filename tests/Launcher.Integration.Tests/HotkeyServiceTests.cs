using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Launcher.Core.Diagnostics;
using Launcher.Core.Integrations;
using Launcher.Diagnostics;
using Launcher.Integrations.Hotkey;
using Xunit;

namespace Launcher.Integration.Tests;

/// <summary>
/// Behavior of <see cref="HotkeyService"/> against a fake Windows hotkey API (ADR-0024). The message loop, command
/// queue and press dispatch are real; only the registration calls are faked, so these tests never touch a real hotkey.
/// </summary>
public sealed class HotkeyServiceTests : IDisposable
{
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(10);

    // Control+Alt+Shift with F24 and F23, keys that practically no application binds.
    private static readonly HotkeyGesture First = new(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, 0x87);
    private static readonly HotkeyGesture Second = new(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, 0x86);

    private readonly FakeHotkeyNative _native = new();
    private readonly MemoryDiagnosticsSink _sink = new();
    private readonly HotkeyService _service;

    public HotkeyServiceTests()
    {
        _service = new HotkeyService(new DiagnosticsService([_sink]), _native);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _service.Dispose();
    }

    [Fact]
    public void The_default_gesture_is_control_win_alt_space()
    {
        var gesture = HotkeyDefaults.LauncherGesture;

        Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Windows, gesture.Modifiers);
        Assert.Equal(0x20u, gesture.VirtualKey);
    }

    [Fact]
    public async Task The_default_gesture_passes_validation()
    {
        var result = await _service.RegisterAsync(HotkeyDefaults.LauncherGesture, Token);

        Assert.Equal(HotkeyRegistrationStatus.Registered, result.Status);
    }

    [Fact]
    public async Task Registering_a_gesture_reports_registered_and_current()
    {
        var result = await _service.RegisterAsync(First, Token);

        Assert.Equal(HotkeyRegistrationStatus.Registered, result.Status);
        Assert.Equal(First, result.Gesture);
        Assert.Null(result.ErrorCode);
        Assert.Equal(First, _service.Current);
        Assert.Equal("register 1", string.Join(", ", _native.Calls));
    }

    [Theory]
    [InlineData(0, 0x87u)]
    [InlineData(7, 0u)]
    [InlineData(7, 0xFFu)]
    [InlineData(7, 0x11u)]
    [InlineData(16, 0x87u)]
    public async Task Invalid_gestures_are_rejected_without_touching_the_system(int modifiers, uint virtualKey)
    {
        var gesture = new HotkeyGesture((HotkeyModifiers)modifiers, virtualKey);

        var result = await _service.RegisterAsync(gesture, Token);

        Assert.Equal(HotkeyRegistrationStatus.InvalidGesture, result.Status);
        Assert.Equal("hotkey.invalid_gesture", result.ErrorCode);
        Assert.Null(_service.Current);
        Assert.Empty(_native.Calls);
    }

    [Fact]
    public async Task Registering_the_same_gesture_again_is_a_no_op()
    {
        await _service.RegisterAsync(First, Token);

        var again = await _service.RegisterAsync(First, Token);

        Assert.Equal(HotkeyRegistrationStatus.Registered, again.Status);
        Assert.Equal("register 1", string.Join(", ", _native.Calls));
    }

    [Fact]
    public async Task Replacing_a_gesture_registers_the_new_one_before_releasing_the_old_one()
    {
        await _service.RegisterAsync(First, Token);

        var result = await _service.RegisterAsync(Second, Token);

        Assert.Equal(HotkeyRegistrationStatus.Registered, result.Status);
        Assert.Equal(Second, _service.Current);
        Assert.Equal("register 1, register 2, unregister 1", string.Join(", ", _native.Calls));
        Assert.Equal("2", string.Join(",", _native.RegisteredIds));
    }

    [Fact]
    public async Task A_failed_replacement_keeps_the_old_hotkey_working()
    {
        await _service.RegisterAsync(First, Token);
        _native.FailRegistrationWith = 1409;

        var result = await _service.RegisterAsync(Second, Token);

        Assert.Equal(HotkeyRegistrationStatus.InUse, result.Status);
        Assert.Equal(Second, result.Gesture);
        Assert.Equal(First, _service.Current);
        Assert.Equal("register 1, register 2", string.Join(", ", _native.Calls));
        Assert.Equal("1", string.Join(",", _native.RegisteredIds));
    }

    [Theory]
    [InlineData(1409, HotkeyRegistrationStatus.InUse, "hotkey.in_use")]
    [InlineData(87, HotkeyRegistrationStatus.InvalidGesture, "hotkey.invalid_gesture")]
    [InlineData(5, HotkeyRegistrationStatus.Failed, "hotkey.register_failed")]
    [InlineData(1234, HotkeyRegistrationStatus.Failed, "hotkey.register_failed")]
    public async Task A_system_failure_becomes_a_typed_logged_result(int win32Error, HotkeyRegistrationStatus status, string code)
    {
        _native.FailRegistrationWith = win32Error;

        var result = await _service.RegisterAsync(First, Token);

        Assert.Equal(status, result.Status);
        Assert.Equal(code, result.ErrorCode);
        Assert.Null(_service.Current);
        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == code);
        Assert.Equal(LogLevel.Warning, logged.Level);
        Assert.Equal(win32Error.ToString(CultureInfo.InvariantCulture), logged.Metadata["win32Error"]);
    }

    [Fact]
    public async Task Unregistering_releases_the_hotkey_and_is_safe_to_repeat()
    {
        await _service.RegisterAsync(First, Token);

        await _service.UnregisterAsync(Token);
        await _service.UnregisterAsync(Token);

        Assert.Null(_service.Current);
        Assert.Equal("register 1, unregister 1", string.Join(", ", _native.Calls));
    }

    [Fact]
    public async Task Unregistering_before_anything_was_registered_does_nothing()
    {
        await _service.UnregisterAsync(Token);

        Assert.Null(_service.Current);
        Assert.Empty(_native.Calls);
    }

    [Fact]
    public async Task A_cancelled_token_throws_and_changes_nothing()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.RegisterAsync(First, cancelled.Token));

        Assert.Null(_service.Current);
        Assert.Empty(_native.Calls);
    }

    [Fact]
    public async Task A_press_raises_Pressed_with_the_gesture_and_a_fresh_timestamp()
    {
        await _service.RegisterAsync(First, Token);
        var pressed = new TaskCompletionSource<HotkeyPressedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.Pressed += (_, args) => pressed.TrySetResult(args);

        var before = Stopwatch.GetTimestamp();
        Assert.True(_service.PostSyntheticPress(1));
        var received = await pressed.Task.WaitAsync(WaitLimit, Token);
        var after = Stopwatch.GetTimestamp();

        Assert.Equal(First, received.Gesture);
        Assert.InRange(received.Timestamp, before, after);
    }

    [Fact]
    public async Task A_press_for_an_old_registration_is_ignored()
    {
        // Registration ids alternate 1, 2, so the second registration uses id 2 and id 1 is stale.
        await _service.RegisterAsync(First, Token);
        await _service.RegisterAsync(Second, Token);
        var received = new ConcurrentQueue<HotkeyGesture>();
        _service.Pressed += (_, args) => received.Enqueue(args.Gesture);

        Assert.True(_service.PostSyntheticPress(1));
        Assert.True(_service.PostSyntheticPress(2));
        await WaitUntilAsync(() => !received.IsEmpty);

        Assert.Equal(Second, Assert.Single(received));
    }

    [Fact]
    public async Task A_failing_handler_is_logged_without_its_message_and_later_presses_still_arrive()
    {
        await _service.RegisterAsync(First, Token);
        var calls = 0;
        var secondArrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.Pressed += (_, _) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                throw new InvalidOperationException("secret handler message");
            }

            secondArrived.TrySetResult();
        };

        Assert.True(_service.PostSyntheticPress(1));
        await WaitUntilAsync(() => _sink.Entries.Any(entry => entry.ErrorCode == "hotkey.handler_failed"));
        Assert.True(_service.PostSyntheticPress(1));
        await secondArrived.Task.WaitAsync(WaitLimit, Token);

        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "hotkey.handler_failed");
        Assert.Equal(LogLevel.Error, logged.Level);
        Assert.Equal("InvalidOperationException", logged.Metadata["exceptionType"]);
        Assert.DoesNotContain(logged.Metadata.Values, value => value.Contains("secret", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Disposing_releases_the_hotkey_and_later_registration_fails_with_a_typed_result()
    {
        await _service.RegisterAsync(First, Token);

        _service.Dispose();
        _service.Dispose();
        var later = await _service.RegisterAsync(Second, Token);

        Assert.Null(_service.Current);
        Assert.Equal("register 1, unregister 1", string.Join(", ", _native.Calls));
        Assert.Equal(HotkeyRegistrationStatus.Failed, later.Status);
        Assert.Equal("hotkey.disposed", later.ErrorCode);
    }

    [Fact]
    public void Disposing_a_service_that_never_started_is_safe()
    {
        _service.Dispose();

        Assert.Empty(_native.Calls);
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
}
