namespace Launcher.Core.Integrations;

/// <summary>
/// The outcome of trying to become the single running launcher instance (ADR-0024).
/// </summary>
public enum SingleInstanceStatus
{
    /// <summary>This process now owns single-instance ownership.</summary>
    Owner,

    /// <summary>Another launcher instance of the same user already owns it.</summary>
    OtherInstanceRunning,

    /// <summary>Ownership could not be determined. The error code says why.</summary>
    Failed,
}
