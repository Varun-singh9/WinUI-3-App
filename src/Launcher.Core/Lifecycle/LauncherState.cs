namespace Launcher.Core.Lifecycle;

/// <summary>
/// Semantic application states (Implementation Specification section 4.2). The UI and Motion System react to
/// these states; business code never starts animations directly (ADR-0006).
/// </summary>
public enum LauncherState
{
    /// <summary>The launcher is not visible and is waiting for activation.</summary>
    Hidden,

    /// <summary>Activation has started and the activation visual is running.</summary>
    Activating,

    /// <summary>The critical runtime resources needed for interactive use are ready.</summary>
    CriticalReady,

    /// <summary>The activation visual is morphing into the command bar.</summary>
    Morphing,

    /// <summary>The command bar is usable and idle.</summary>
    Ready,

    /// <summary>The prefix popup is open.</summary>
    PrefixOpen,

    /// <summary>The customization or options menu is open.</summary>
    MenuOpen,

    /// <summary>A search is in progress.</summary>
    Searching,

    /// <summary>An action is executing.</summary>
    Executing,

    /// <summary>The launcher is closing.</summary>
    Closing,

    /// <summary>A recoverable error is being handled.</summary>
    ErrorRecovery,
}
