namespace Launcher.Core.Integrations;

/// <summary>
/// The typed outcome of registering a global hotkey. A failed registration is a value, not an exception, so the
/// launcher can keep running and expose a recoverable configuration error (Implementation Specification section 7).
/// </summary>
/// <param name="Status">What happened.</param>
/// <param name="Gesture">The gesture that was requested.</param>
/// <param name="ErrorCode">A safe, stable error code when registration did not succeed; otherwise <see langword="null"/>.</param>
public sealed record HotkeyRegistrationResult(HotkeyRegistrationStatus Status, HotkeyGesture Gesture, string? ErrorCode);
