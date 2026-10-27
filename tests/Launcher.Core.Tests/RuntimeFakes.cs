using System.Diagnostics;
using Launcher.Core.Integrations;

namespace Launcher.Core.Tests;

/// <summary>A thread-safe, ordered record of calls made to the fakes, so tests can check the order of operations.</summary>
internal sealed class CallLog
{
    private readonly object _gate = new();
    private readonly List<string> _items = [];

    public string Joined
    {
        get
        {
            lock (_gate)
            {
                return string.Join(", ", _items);
            }
        }
    }

    public void Add(string call)
    {
        lock (_gate)
        {
            _items.Add(call);
        }
    }
}

/// <summary>A stand-in for the Win32 hotkey service: it records calls, can fail or throw, and can raise a press.</summary>
internal sealed class FakeHotkeyService(CallLog log) : IHotkeyService
{
    private HotkeyGesture? _requested;

    public event EventHandler<HotkeyPressedEventArgs>? Pressed;

    public bool HasSubscribers => Pressed is not null;

    public HotkeyGesture? Current { get; private set; }

    public HotkeyGesture? RequestedGesture => _requested;

    /// <summary>Gets or sets the result registrations return. When null, registration succeeds.</summary>
    public HotkeyRegistrationResult? NextResult { get; set; }

    /// <summary>Gets or sets an exception that registration throws, to model an unexpected failure.</summary>
    public Exception? ThrowOnRegister { get; set; }

    /// <summary>Gets or sets an action run when registration starts, for example to cancel a token mid-start.</summary>
    public Action? OnRegister { get; set; }

    public Task<HotkeyRegistrationResult> RegisterAsync(HotkeyGesture gesture, CancellationToken cancellationToken)
    {
        log.Add("hotkey.register");
        _requested = gesture;
        OnRegister?.Invoke();
        cancellationToken.ThrowIfCancellationRequested();

        if (ThrowOnRegister is not null)
        {
            throw ThrowOnRegister;
        }

        var result = NextResult ?? new HotkeyRegistrationResult(HotkeyRegistrationStatus.Registered, gesture, null);
        if (result.Status == HotkeyRegistrationStatus.Registered)
        {
            Current = gesture;
        }

        return Task.FromResult(result);
    }

    public Task UnregisterAsync(CancellationToken cancellationToken)
    {
        log.Add("hotkey.unregister");
        Current = null;
        return Task.CompletedTask;
    }

    public void RaisePressed()
    {
        var gesture = _requested ?? new HotkeyGesture(HotkeyModifiers.Control, 0x41);
        Pressed?.Invoke(this, new HotkeyPressedEventArgs(gesture, Stopwatch.GetTimestamp()));
    }
}

/// <summary>A stand-in for single-instance ownership: configurable outcomes, recorded calls, and a raisable signal.</summary>
internal sealed class FakeSingleInstanceOwner(CallLog log, FakeHotkeyService hotkey) : ISingleInstanceOwner
{
    public event EventHandler? ActivationRequested;

    public bool HasSubscribers => ActivationRequested is not null;

    public bool IsOwner { get; private set; }

    /// <summary>Gets or sets the result acquisition returns. When unchanged, this process becomes the owner.</summary>
    public SingleInstanceResult NextAcquire { get; set; } = new(SingleInstanceStatus.Owner, null);

    /// <summary>Gets or sets whether signaling the owner succeeds.</summary>
    public bool SignalResult { get; set; } = true;

    /// <summary>Gets a value indicating whether both event sources already had a subscriber when acquisition happened.</summary>
    public bool SubscribedBeforeAcquire { get; private set; }

    public SingleInstanceResult TryAcquire()
    {
        log.Add("single.acquire");
        SubscribedBeforeAcquire = HasSubscribers && hotkey.HasSubscribers;
        IsOwner = NextAcquire.Status == SingleInstanceStatus.Owner;
        return NextAcquire;
    }

    public bool TrySignalOwner()
    {
        log.Add("single.signal");
        return SignalResult;
    }

    public void Release()
    {
        log.Add("single.release");
        IsOwner = false;
    }

    public void RaiseActivationRequested()
    {
        ActivationRequested?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>A stand-in for the optional integrations that can stay pending, fail, ignore cancellation or block its thread.</summary>
internal sealed class FakeIntegrationManager : IIntegrationManager
{
    private int _started;
    private int _observedCancellation;

    public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Started => Volatile.Read(ref _started);

    public bool ObservedCancellation => Volatile.Read(ref _observedCancellation) == 1;

    /// <summary>Gets or sets an exception initialization throws.</summary>
    public Exception? Throw { get; set; }

    /// <summary>Gets or sets a value indicating whether initialization ignores its cancellation token.</summary>
    public bool IgnoreCancellation { get; set; }

    /// <summary>Gets or sets a value indicating whether initialization blocks its calling thread instead of awaiting.</summary>
    public bool BlockSynchronously { get; set; }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _started);

        if (Throw is not null)
        {
            throw Throw;
        }

        if (BlockSynchronously)
        {
            Gate.Task.Wait();
            return;
        }

        if (IgnoreCancellation)
        {
            await Gate.Task.ConfigureAwait(false);
            return;
        }

        try
        {
            await Gate.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Exchange(ref _observedCancellation, 1);
            throw;
        }
    }

    public bool IsAvailable(string integrationId)
    {
        return false;
    }
}
