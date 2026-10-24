namespace Launcher.Integrations.Hotkey;

/// <summary>
/// Stable, safe error codes reported by <see cref="HotkeyService"/> in results and diagnostics.
/// </summary>
internal static class HotkeyErrorCodes
{
    /// <summary>Another application already owns the gesture.</summary>
    public const string InUse = "hotkey.in_use";

    /// <summary>The gesture is not valid, or the system rejected its parameters.</summary>
    public const string InvalidGesture = "hotkey.invalid_gesture";

    /// <summary>The system refused the registration for another reason.</summary>
    public const string RegisterFailed = "hotkey.register_failed";

    /// <summary>The service has been disposed.</summary>
    public const string Disposed = "hotkey.disposed";

    /// <summary>The message-loop thread did not start or ended unexpectedly.</summary>
    public const string LoopFailed = "hotkey.loop_failed";

    /// <summary>A handler of the <c>Pressed</c> event threw.</summary>
    public const string HandlerFailed = "hotkey.handler_failed";
}
