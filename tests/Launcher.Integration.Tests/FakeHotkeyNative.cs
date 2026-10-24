using Launcher.Core.Integrations;
using Launcher.Integrations.Hotkey;

namespace Launcher.Integration.Tests;

/// <summary>
/// A stand-in for the Windows hotkey API: it records every call and can be told to fail registrations, so the
/// service's behavior can be tested on any machine without touching real global hotkeys.
/// </summary>
internal sealed class FakeHotkeyNative : IHotkeyNative
{
    private readonly object _gate = new();
    private readonly List<string> _calls = [];
    private readonly Dictionary<int, (HotkeyModifiers Modifiers, uint VirtualKey)> _registered = [];

    /// <summary>Gets or sets the Win32 error code that registrations fail with. Zero means they succeed.</summary>
    public int FailRegistrationWith { get; set; }

    /// <summary>Gets a snapshot of the calls made so far, for example <c>register 1</c> or <c>unregister 1</c>.</summary>
    public string[] Calls
    {
        get
        {
            lock (_gate)
            {
                return [.. _calls];
            }
        }
    }

    /// <summary>Gets the ids that are registered right now, in ascending order.</summary>
    public int[] RegisteredIds
    {
        get
        {
            lock (_gate)
            {
                return [.. _registered.Keys.Order()];
            }
        }
    }

    public bool TryRegister(int id, HotkeyModifiers modifiers, uint virtualKey, out int errorCode)
    {
        lock (_gate)
        {
            _calls.Add($"register {id}");

            if (FailRegistrationWith != 0)
            {
                errorCode = FailRegistrationWith;
                return false;
            }

            _registered[id] = (modifiers, virtualKey);
            errorCode = 0;
            return true;
        }
    }

    public void Unregister(int id)
    {
        lock (_gate)
        {
            _calls.Add($"unregister {id}");
            _registered.Remove(id);
        }
    }
}
