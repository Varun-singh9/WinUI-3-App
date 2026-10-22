namespace Launcher.Core.Lifecycle;

/// <summary>
/// Exposes the current semantic state and state changes to the UI (Implementation Specification section 5).
/// </summary>
public interface ILauncherLifecycle
{
    /// <summary>Raised after the launcher enters a new state.</summary>
    event EventHandler<LauncherStateChangedEventArgs>? StateChanged;

    /// <summary>Gets the current semantic state.</summary>
    LauncherState State { get; }
}
