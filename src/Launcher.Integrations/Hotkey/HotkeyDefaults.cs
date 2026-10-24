using Launcher.Core.Integrations;

namespace Launcher.Integrations.Hotkey;

/// <summary>
/// The launcher's built-in hotkey choices (ADR-0024). The service never reads these itself: the composition root
/// passes a gesture to <see cref="IHotkeyService.RegisterAsync"/>, so the default can change without touching the service.
/// </summary>
public static class HotkeyDefaults
{
    private const uint VirtualKeySpace = 0x20;

    /// <summary>
    /// Gets Ctrl+Win+Alt+Space, chosen to avoid clashing with PowerToys Run and other launchers. It has not been
    /// verified on every machine, so a failed registration must be handled by the caller.
    /// </summary>
    public static HotkeyGesture LauncherGesture { get; } =
        new(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Windows, VirtualKeySpace);
}
