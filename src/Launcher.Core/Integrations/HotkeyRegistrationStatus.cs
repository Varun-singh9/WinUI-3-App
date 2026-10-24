namespace Launcher.Core.Integrations;

/// <summary>
/// The outcome of a hotkey registration attempt (ADR-0024).
/// </summary>
public enum HotkeyRegistrationStatus
{
    /// <summary>The gesture is registered and presses are reported.</summary>
    Registered,

    /// <summary>Another application already owns the gesture.</summary>
    InUse,

    /// <summary>The gesture is not valid, for example it has no main key.</summary>
    InvalidGesture,

    /// <summary>Registration failed for another reason. The error code says why.</summary>
    Failed,
}
