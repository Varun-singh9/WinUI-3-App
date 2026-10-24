using System.Runtime.InteropServices;
using Launcher.Core.Integrations;

namespace Launcher.Integrations.Hotkey;

/// <summary>
/// The real user32 implementation of <see cref="IHotkeyNative"/>. It registers with no window handle, so the
/// operating system posts <c>WM_HOTKEY</c> to the registering thread's message queue.
/// </summary>
internal sealed class Win32HotkeyNative : IHotkeyNative
{
    /// <inheritdoc />
    public bool TryRegister(int id, HotkeyModifiers modifiers, uint virtualKey, out int errorCode)
    {
        // MOD_NOREPEAT: holding the keys down must not raise a stream of presses.
        var native = NativeMethods.ModNoRepeat;
        if (modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            native |= NativeMethods.ModAlt;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Control))
        {
            native |= NativeMethods.ModControl;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            native |= NativeMethods.ModShift;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            native |= NativeMethods.ModWin;
        }

        if (NativeMethods.RegisterHotKey(0, id, native, virtualKey))
        {
            errorCode = 0;
            return true;
        }

        errorCode = Marshal.GetLastPInvokeError();
        return false;
    }

    /// <inheritdoc />
    public void Unregister(int id)
    {
        _ = NativeMethods.UnregisterHotKey(0, id);
    }
}
