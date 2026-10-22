namespace Launcher.PluginContracts;

/// <summary>
/// Outcome reported in an <see cref="IpcResponse"/>. The set of values is provisional until the plugin phase.
/// </summary>
public enum IpcStatus
{
    /// <summary>The operation succeeded.</summary>
    Success,

    /// <summary>The operation failed; see the error code.</summary>
    Error,

    /// <summary>The operation was cancelled.</summary>
    Cancelled,

    /// <summary>The operation did not finish within its timeout.</summary>
    TimedOut,
}
