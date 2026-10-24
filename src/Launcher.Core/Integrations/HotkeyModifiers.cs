namespace Launcher.Core.Integrations;

/// <summary>
/// The modifier keys of a <see cref="HotkeyGesture"/>. Values combine as flags.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
    /// <summary>No modifier key.</summary>
    None = 0,

    /// <summary>The Alt key.</summary>
    Alt = 1,

    /// <summary>The Control key.</summary>
    Control = 2,

    /// <summary>The Shift key.</summary>
    Shift = 4,

    /// <summary>The Windows logo key.</summary>
    Windows = 8,
}
