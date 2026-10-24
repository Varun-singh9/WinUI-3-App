using System.Security.AccessControl;
using System.Security.Principal;
using Launcher.Core.Diagnostics;
using Launcher.Core.Integrations;

namespace Launcher.Integrations.SingleInstance;

/// <summary>
/// Single-instance ownership for the current user, built on a named mutex and a named event (ADR-0024).
/// </summary>
/// <remarks>
/// <para>
/// Ownership is decided by who created the named mutex first. The mutex is never waited on or "owned" in the
/// Win32 sense, so it has no thread affinity and can be released from any thread. The operating system removes it
/// when the owning process ends, so a crash leaves no stale lock.
/// </para>
/// <para>
/// The event is a pure signal: it carries no data. Both objects are created with an ACL that admits only the
/// current user, and both live in the per-session <c>Local\</c> namespace. The event is created before the mutex,
/// so whenever the mutex exists the event exists too and a second instance can never signal into a gap.
/// </para>
/// <para>
/// Expected failures are returned as values and written to diagnostics with an error code. Nothing here throws
/// into the caller for them.
/// </para>
/// </remarks>
public sealed class SingleInstanceOwner : ISingleInstanceOwner, IDisposable
{
    private const string Component = "SingleInstance";

    private readonly SingleInstanceNames _names;
    private readonly IDiagnostics _diagnostics;
    private readonly Lock _gate = new();

    private Mutex? _mutex;
    private EventWaitHandle? _activationEvent;
    private RegisteredWaitHandle? _registeredWait;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleInstanceOwner"/> class.
    /// </summary>
    /// <param name="names">The object names to use.</param>
    /// <param name="diagnostics">Where failures and notable events are written.</param>
    public SingleInstanceOwner(SingleInstanceNames names, IDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(diagnostics);

        _names = names;
        _diagnostics = diagnostics;
    }

    /// <inheritdoc />
    public event EventHandler? ActivationRequested;

    /// <inheritdoc />
    public bool IsOwner
    {
        get
        {
            lock (_gate)
            {
                return _mutex is not null;
            }
        }
    }

    /// <inheritdoc />
    public SingleInstanceResult TryAcquire()
    {
        lock (_gate)
        {
            if (_mutex is not null)
            {
                return new SingleInstanceResult(SingleInstanceStatus.Owner, null);
            }

            EventWaitHandle? activationEvent = null;
            Mutex? mutex = null;

            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var userSid = identity.User ?? throw new InvalidOperationException("The current user has no security identifier.");

                var eventSecurity = new EventWaitHandleSecurity();
                eventSecurity.AddAccessRule(new EventWaitHandleAccessRule(userSid, EventWaitHandleRights.FullControl, AccessControlType.Allow));

                var mutexSecurity = new MutexSecurity();
                mutexSecurity.AddAccessRule(new MutexAccessRule(userSid, MutexRights.FullControl, AccessControlType.Allow));

                // Event first, mutex second: see the remarks on this class.
                activationEvent = EventWaitHandleAcl.Create(false, EventResetMode.AutoReset, _names.EventName, out _, eventSecurity);
                mutex = MutexAcl.Create(false, _names.MutexName, out var createdNew, mutexSecurity);

                if (!createdNew)
                {
                    Write(LogLevel.Information, "single_instance.other_instance_running", null);
                    return new SingleInstanceResult(SingleInstanceStatus.OtherInstanceRunning, null);
                }

                _registeredWait = ThreadPool.RegisterWaitForSingleObject(
                    activationEvent,
                    OnActivationSignaled,
                    null,
                    Timeout.Infinite,
                    executeOnlyOnce: false);

                _activationEvent = activationEvent;
                _mutex = mutex;
                activationEvent = null;
                mutex = null;

                Write(LogLevel.Information, "single_instance.acquired", null);
                return new SingleInstanceResult(SingleInstanceStatus.Owner, null);
            }
            catch (UnauthorizedAccessException)
            {
                return Failed(SingleInstanceErrorCodes.AccessDenied);
            }
            catch (Exception exception) when (exception is WaitHandleCannotBeOpenedException or IOException or ArgumentException or InvalidOperationException or PlatformNotSupportedException)
            {
                return Failed(SingleInstanceErrorCodes.CreateFailed);
            }
            finally
            {
                // Runs for every path except a successful acquire, where both locals were handed over and set to null.
                activationEvent?.Dispose();
                mutex?.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public bool TrySignalOwner()
    {
        try
        {
            if (!EventWaitHandleAcl.TryOpenExisting(_names.EventName, EventWaitHandleRights.Modify, out var signal))
            {
                Write(LogLevel.Debug, "single_instance.no_owner_to_signal", null);
                return false;
            }

            using (signal)
            {
                if (signal.Set())
                {
                    Write(LogLevel.Information, "single_instance.signal_sent", null);
                    return true;
                }
            }

            Write(LogLevel.Warning, "single_instance.signal_not_set", SingleInstanceErrorCodes.SignalFailed);
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Write(LogLevel.Warning, "single_instance.signal_failed", SingleInstanceErrorCodes.AccessDenied);
            return false;
        }
        catch (Exception exception) when (exception is WaitHandleCannotBeOpenedException or IOException or ArgumentException or PlatformNotSupportedException)
        {
            Write(LogLevel.Warning, "single_instance.signal_failed", SingleInstanceErrorCodes.SignalFailed);
            return false;
        }
    }

    /// <inheritdoc />
    public void Release()
    {
        lock (_gate)
        {
            if (_mutex is null)
            {
                return;
            }

            _registeredWait?.Unregister(null);
            _registeredWait = null;

            _activationEvent?.Dispose();
            _activationEvent = null;

            _mutex.Dispose();
            _mutex = null;

            Write(LogLevel.Information, "single_instance.released", null);
        }
    }

    /// <summary>Releases ownership, if held.</summary>
    public void Dispose()
    {
        Release();
    }

    private void OnActivationSignaled(object? state, bool timedOut)
    {
        if (timedOut)
        {
            return;
        }

        var handler = ActivationRequested;
        if (handler is null)
        {
            return;
        }

        try
        {
            handler(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            // The handler is foreign code running on a thread-pool thread. An exception escaping here would end the
            // whole process, so it is recorded and swallowed. Only the exception type is logged, never its message.
            Write(
                LogLevel.Error,
                "single_instance.handler_failed",
                SingleInstanceErrorCodes.HandlerFailed,
                new Dictionary<string, string> { ["exceptionType"] = exception.GetType().Name });
        }
    }

    private SingleInstanceResult Failed(string errorCode)
    {
        Write(LogLevel.Error, "single_instance.acquire_failed", errorCode);
        return new SingleInstanceResult(SingleInstanceStatus.Failed, errorCode);
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
}
