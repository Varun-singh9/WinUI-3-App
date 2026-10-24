using Launcher.Core.Integrations;

namespace Launcher.Integrations.Hotkey;

/// <summary>
/// The operating-system calls behind <see cref="HotkeyService"/>, so tests can replace them. Both members are only
/// ever called on the service's message-loop thread, because a hotkey registered without a window belongs to the
/// thread that registered it.
/// </summary>
internal interface IHotkeyNative
{
    /// <summary>Tries to register a hotkey on the calling thread.</summary>
    /// <param name="id">The registration identifier.</param>
    /// <param name="modifiers">The modifier keys.</param>
    /// <param name="virtualKey">The main key.</param>
    /// <param name="errorCode">The Win32 error code when registration fails; otherwise 0.</param>
    /// <returns><see langword="true"/> when the hotkey is registered.</returns>
    bool TryRegister(int id, HotkeyModifiers modifiers, uint virtualKey, out int errorCode);

    /// <summary>Releases a registration made on the calling thread. Failure is ignored: it means the registration is already gone.</summary>
    /// <param name="id">The registration identifier.</param>
    void Unregister(int id);
}
