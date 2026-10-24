using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Launcher.Core.Diagnostics;
using Launcher.Core.Integrations;

namespace Launcher.Integrations.Hotkey;

/// <summary>
/// Registers one global hotkey with <c>RegisterHotKey</c> and reports presses (Implementation Specification
/// section 7, ADR-0024).
/// </summary>
/// <remarks>
/// <para>
/// A hotkey registered without a window belongs to the thread that registered it and is delivered as
/// <c>WM_HOTKEY</c> to that thread's queue. So the service runs one dedicated background message-loop thread, started
/// on the first registration. Every registration, unregistration and press runs on that thread, and callers talk to
/// it through a command queue.
/// </para>
/// <para>
/// There is no keyboard hook and no input interception. Expected failures (gesture in use, invalid gesture, the
/// system refusing) are returned as <see cref="HotkeyRegistrationResult"/> values and logged with an error code; they
/// are never thrown. Replacing a gesture registers the new one before releasing the old one, so a failed replacement
/// leaves the old hotkey working.
/// </para>
/// </remarks>
public sealed class HotkeyService : IHotkeyService, IDisposable
{
    private const string Component = "Hotkey";
    private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(5);

    private readonly IDiagnostics _diagnostics;
    private readonly IHotkeyNative _native;
    private readonly ConcurrentQueue<Command> _commands = new();
    private readonly Lock _gate = new();

    private Thread? _thread;
    private Task<bool>? _ready;
    private uint _threadId;
    private volatile bool _loopEnded;
    private bool _disposed;

    // Written only on the message-loop thread.
    private int _activeId;
    private HotkeyGesture? _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyService"/> class that uses the real Windows hotkey API.
    /// </summary>
    /// <param name="diagnostics">Where failures and notable events are written.</param>
    public HotkeyService(IDiagnostics diagnostics)
        : this(diagnostics, new Win32HotkeyNative())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyService"/> class with a replaceable system layer (for tests).
    /// </summary>
    /// <param name="diagnostics">Where failures and notable events are written.</param>
    /// <param name="native">The system calls to use.</param>
    internal HotkeyService(IDiagnostics diagnostics, IHotkeyNative native)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(native);

        _diagnostics = diagnostics;
        _native = native;
    }

    /// <inheritdoc />
    public event EventHandler<HotkeyPressedEventArgs>? Pressed;

    /// <inheritdoc />
    public HotkeyGesture? Current => Volatile.Read(ref _current);

    /// <inheritdoc />
    public async Task<HotkeyRegistrationResult> RegisterAsync(HotkeyGesture gesture, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gesture);
        cancellationToken.ThrowIfCancellationRequested();

        // An invalid gesture needs neither the thread nor the system.
        if (!IsValid(gesture))
        {
            Write(LogLevel.Warning, "hotkey.registration_rejected", HotkeyErrorCodes.InvalidGesture);
            return new HotkeyRegistrationResult(HotkeyRegistrationStatus.InvalidGesture, gesture, HotkeyErrorCodes.InvalidGesture);
        }

        var startError = await EnsureLoopAsync(cancellationToken).ConfigureAwait(false);
        if (startError is not null)
        {
            Write(LogLevel.Warning, "hotkey.registration_failed", startError);
            return new HotkeyRegistrationResult(HotkeyRegistrationStatus.Failed, gesture, startError);
        }

        var command = new Command<HotkeyRegistrationResult>(
            () => RegisterCore(gesture),
            () => new HotkeyRegistrationResult(HotkeyRegistrationStatus.Failed, gesture, HotkeyErrorCodes.Disposed),
            cancellationToken);
        Submit(command);

        // Cancelling stops the wait. If the command has not started yet it is skipped; if it already ran, Current
        // tells the truth about what is registered.
        return await command.Completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UnregisterAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            // Never started, or already disposed (disposal released the hotkey): nothing is registered.
            if (_disposed || _ready is null)
            {
                return;
            }
        }

        if (await EnsureLoopAsync(cancellationToken).ConfigureAwait(false) is not null)
        {
            return;
        }

        var command = new Command<bool>(
            () =>
            {
                UnregisterCore();
                return true;
            },
            () => true,
            cancellationToken);
        Submit(command);
        await command.Completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Releases the hotkey and stops the message-loop thread. Safe to call more than once. Blocks for a short, bounded time.
    /// </summary>
    public void Dispose()
    {
        Thread? thread;
        Task<bool>? ready;

        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            thread = _thread;
            ready = _ready;
        }

        if (thread is null || ready is null)
        {
            return;
        }

        if (ready.Wait(StartTimeout) && !_loopEnded)
        {
            _ = NativeMethods.PostThreadMessageW(_threadId, NativeMethods.WmQuit, 0, 0);
        }

        // Disposing from inside a Pressed handler runs on the loop thread itself, which cannot wait for itself.
        if (Environment.CurrentManagedThreadId != thread.ManagedThreadId && !thread.Join(StopTimeout))
        {
            Write(LogLevel.Warning, "hotkey.stop_timeout", HotkeyErrorCodes.LoopFailed);
        }
    }

    /// <summary>Posts a <c>WM_HOTKEY</c> for a registration id to the loop thread, as the system would. For tests only.</summary>
    /// <param name="id">The registration id.</param>
    /// <returns><see langword="true"/> when the message was posted.</returns>
    internal bool PostSyntheticPress(int id)
    {
        return !_loopEnded && NativeMethods.PostThreadMessageW(_threadId, NativeMethods.WmHotkey, (nuint)id, 0);
    }

    private static bool IsValid(HotkeyGesture gesture)
    {
        const HotkeyModifiers all = HotkeyModifiers.Alt | HotkeyModifiers.Control | HotkeyModifiers.Shift | HotkeyModifiers.Windows;

        // A global hotkey without a modifier would take a plain key away from every application.
        if (gesture.Modifiers == HotkeyModifiers.None || (gesture.Modifiers & ~all) != 0)
        {
            return false;
        }

        // Virtual-key codes run from 1 to 254. Modifier keys cannot be the main key.
        return gesture.VirtualKey is >= 1 and <= 0xFE && !IsModifierKey(gesture.VirtualKey);
    }

    private static bool IsModifierKey(uint virtualKey)
    {
        // Shift, Control, Alt, left and right Windows keys, and the left/right Shift, Control and Alt variants.
        return virtualKey is 0x10 or 0x11 or 0x12 or 0x5B or 0x5C or (>= 0xA0 and <= 0xA5);
    }

    private static (HotkeyRegistrationStatus Status, string Code) Classify(int error)
    {
        return error switch
        {
            NativeMethods.ErrorHotkeyAlreadyRegistered => (HotkeyRegistrationStatus.InUse, HotkeyErrorCodes.InUse),
            NativeMethods.ErrorInvalidParameter => (HotkeyRegistrationStatus.InvalidGesture, HotkeyErrorCodes.InvalidGesture),
            _ => (HotkeyRegistrationStatus.Failed, HotkeyErrorCodes.RegisterFailed),
        };
    }

    private async Task<string?> EnsureLoopAsync(CancellationToken cancellationToken)
    {
        Task<bool> ready;

        lock (_gate)
        {
            if (_disposed)
            {
                return HotkeyErrorCodes.Disposed;
            }

            _ready ??= StartLoop();
            ready = _ready;
        }

        try
        {
            return await ready.WaitAsync(StartTimeout, cancellationToken).ConfigureAwait(false) ? null : HotkeyErrorCodes.LoopFailed;
        }
        catch (TimeoutException)
        {
            return HotkeyErrorCodes.LoopFailed;
        }
    }

    private Task<bool> StartLoop()
    {
        var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() => RunLoop(ready))
        {
            IsBackground = true,
            Name = "Launcher.Hotkey",
        };

        _thread = thread;
        thread.Start();
        return ready.Task;
    }

    private void Submit(Command command)
    {
        if (_loopEnded)
        {
            command.Abandon();
            return;
        }

        _commands.Enqueue(command);
        if (!NativeMethods.PostThreadMessageW(_threadId, NativeMethods.WmApp, 0, 0))
        {
            command.Abandon();
        }
    }

    private void RunLoop(TaskCompletionSource<bool> ready)
    {
        try
        {
            // Peeking creates this thread's message queue, so anything posted after "ready" cannot be lost.
            _ = NativeMethods.PeekMessageW(out _, 0, NativeMethods.WmUser, NativeMethods.WmUser, NativeMethods.PmNoRemove);
            _threadId = NativeMethods.GetCurrentThreadId();
            ready.TrySetResult(true);

            while (true)
            {
                var result = NativeMethods.GetMessageW(out var message, 0, 0, 0);
                if (result == 0)
                {
                    break;
                }

                if (result < 0)
                {
                    Write(LogLevel.Error, "hotkey.loop_failed", HotkeyErrorCodes.LoopFailed);
                    break;
                }

                switch (message.Message)
                {
                    case NativeMethods.WmApp:
                        RunCommands();
                        break;
                    case NativeMethods.WmHotkey:
                        OnHotkey((int)message.WParam);
                        break;
                }
            }
        }
        catch (Exception exception)
        {
            // This thread must never throw: an unhandled exception here would end the whole process.
            Write(LogLevel.Error, "hotkey.loop_failed", HotkeyErrorCodes.LoopFailed, ExceptionMetadata(exception));
        }
        finally
        {
            _loopEnded = true;

            try
            {
                UnregisterCore();
            }
            catch (Exception exception)
            {
                Write(LogLevel.Error, "hotkey.unregister_failed", HotkeyErrorCodes.LoopFailed, ExceptionMetadata(exception));
            }

            while (_commands.TryDequeue(out var command))
            {
                command.Abandon();
            }

            ready.TrySetResult(false);
        }
    }

    private void RunCommands()
    {
        while (_commands.TryDequeue(out var command))
        {
            command.Run();
        }
    }

    private HotkeyRegistrationResult RegisterCore(HotkeyGesture gesture)
    {
        if (gesture == _current)
        {
            return new HotkeyRegistrationResult(HotkeyRegistrationStatus.Registered, gesture, null);
        }

        // Alternate between two ids so the new gesture can be registered while the old one still is.
        var newId = _activeId == 1 ? 2 : 1;

        if (!_native.TryRegister(newId, gesture.Modifiers, gesture.VirtualKey, out var error))
        {
            var (status, code) = Classify(error);
            Write(
                LogLevel.Warning,
                "hotkey.registration_failed",
                code,
                new Dictionary<string, string> { ["win32Error"] = error.ToString(CultureInfo.InvariantCulture) });
            return new HotkeyRegistrationResult(status, gesture, code);
        }

        if (_activeId != 0)
        {
            _native.Unregister(_activeId);
        }

        _activeId = newId;
        Volatile.Write(ref _current, gesture);
        Write(LogLevel.Information, "hotkey.registered", null);
        return new HotkeyRegistrationResult(HotkeyRegistrationStatus.Registered, gesture, null);
    }

    private void UnregisterCore()
    {
        if (_activeId == 0)
        {
            return;
        }

        _native.Unregister(_activeId);
        _activeId = 0;
        Volatile.Write(ref _current, null);
        Write(LogLevel.Information, "hotkey.unregistered", null);
    }

    private void OnHotkey(int id)
    {
        var timestamp = Stopwatch.GetTimestamp();
        var gesture = _current;

        // A press queued for an older registration, for example just before a replacement, is ignored.
        if (gesture is null || id != _activeId)
        {
            return;
        }

        Write(LogLevel.Debug, "hotkey.pressed", null);

        var handler = Pressed;
        if (handler is null)
        {
            return;
        }

        try
        {
            handler(this, new HotkeyPressedEventArgs(gesture, timestamp));
        }
        catch (Exception exception)
        {
            // The handler is foreign code on the loop thread; an exception must not end the process or the loop.
            // Only the exception type is logged, never its message.
            Write(LogLevel.Error, "hotkey.handler_failed", HotkeyErrorCodes.HandlerFailed, ExceptionMetadata(exception));
        }
    }

    private static Dictionary<string, string> ExceptionMetadata(Exception exception)
    {
        return new Dictionary<string, string> { ["exceptionType"] = exception.GetType().Name };
    }

    private void Write(LogLevel level, string eventId, string? errorCode, IReadOnlyDictionary<string, string>? metadata = null)
    {
        _diagnostics.Write(new LogEntry(
            DateTimeOffset.UtcNow,
            level,
            Component,
            eventId,
            null,
            null,
            errorCode,
            metadata ?? new Dictionary<string, string>()));
    }

    private abstract class Command
    {
        public abstract void Run();

        public abstract void Abandon();
    }

    private sealed class Command<T>(Func<T> work, Func<T> abandoned, CancellationToken cancellationToken) : Command
    {
        public TaskCompletionSource<T> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override void Run()
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Completion.TrySetCanceled(cancellationToken);
                return;
            }

            try
            {
                Completion.TrySetResult(work());
            }
            catch (Exception exception)
            {
                // Hand an unexpected failure to the awaiting caller instead of letting it escape on the loop thread.
                Completion.TrySetException(exception);
            }
        }

        public override void Abandon()
        {
            Completion.TrySetResult(abandoned());
        }
    }
}
