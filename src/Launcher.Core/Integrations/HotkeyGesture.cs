namespace Launcher.Core.Integrations;

/// <summary>
/// A global hotkey: modifier keys plus one key (ADR-0024). The gesture is data; the implementation decides whether
/// it is valid and whether it can be registered on this machine.
/// </summary>
/// <param name="Modifiers">The modifier keys that must be held.</param>
/// <param name="VirtualKey">The Windows virtual-key code of the main key.</param>
public sealed record HotkeyGesture(HotkeyModifiers Modifiers, uint VirtualKey);
