namespace Launcher.Core.Integrations;

/// <summary>
/// Describes one press of the registered global hotkey.
/// </summary>
public sealed class HotkeyPressedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyPressedEventArgs"/> class.
    /// </summary>
    /// <param name="gesture">The gesture that was pressed.</param>
    /// <param name="timestamp">The <see cref="System.Diagnostics.Stopwatch.GetTimestamp"/> value taken when the press was received.</param>
    public HotkeyPressedEventArgs(HotkeyGesture gesture, long timestamp)
    {
        Gesture = gesture;
        Timestamp = timestamp;
    }

    /// <summary>Gets the gesture that was pressed.</summary>
    public HotkeyGesture Gesture { get; }

    /// <summary>
    /// Gets the <see cref="System.Diagnostics.Stopwatch.GetTimestamp"/> value taken when the press was received,
    /// so hotkey-to-input-ready time can be measured separately from optional provider readiness.
    /// </summary>
    public long Timestamp { get; }
}
