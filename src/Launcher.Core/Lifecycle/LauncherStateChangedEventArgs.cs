namespace Launcher.Core.Lifecycle;

/// <summary>
/// Describes one transition between semantic launcher states.
/// </summary>
public sealed class LauncherStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherStateChangedEventArgs"/> class.
    /// </summary>
    /// <param name="previous">The state that was left.</param>
    /// <param name="current">The state that was entered.</param>
    public LauncherStateChangedEventArgs(LauncherState previous, LauncherState current)
    {
        Previous = previous;
        Current = current;
    }

    /// <summary>Gets the state that was left.</summary>
    public LauncherState Previous { get; }

    /// <summary>Gets the state that was entered.</summary>
    public LauncherState Current { get; }
}
