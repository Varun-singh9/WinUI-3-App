namespace Launcher.Core.Integrations;

/// <summary>
/// Registers the launcher's global hotkey and reports presses (Implementation Specification section 7, ADR-0024).
/// The implementation lives in <c>Launcher.Integrations</c>; Core only reacts to <see cref="Pressed"/>.
/// An expected failure such as a gesture that is already in use is returned as a
/// <see cref="HotkeyRegistrationResult"/> and never thrown. The only exception members throw on purpose is
/// <see cref="OperationCanceledException"/> when the caller cancels.
/// </summary>
public interface IHotkeyService
{
    /// <summary>
    /// Raised when the registered gesture is pressed. It may be raised on a background thread, so handlers must
    /// marshal to their own context and must not block.
    /// </summary>
    event EventHandler<HotkeyPressedEventArgs>? Pressed;

    /// <summary>Gets the gesture that is currently registered, or <see langword="null"/> when none is.</summary>
    HotkeyGesture? Current { get; }

    /// <summary>
    /// Registers a gesture. Calling it again is safe and is how a failed registration is retried or rebound. If the
    /// new gesture cannot be registered, a gesture that was registered before stays registered.
    /// </summary>
    /// <param name="gesture">The gesture to register.</param>
    /// <param name="cancellationToken">Cancels the registration.</param>
    /// <returns>A task with the typed registration outcome.</returns>
    Task<HotkeyRegistrationResult> RegisterAsync(HotkeyGesture gesture, CancellationToken cancellationToken);

    /// <summary>Releases the registered gesture, if any. Safe to call when nothing is registered.</summary>
    /// <param name="cancellationToken">Cancels the wait for the release to finish.</param>
    /// <returns>A task that completes when the gesture is no longer registered.</returns>
    Task UnregisterAsync(CancellationToken cancellationToken);
}
