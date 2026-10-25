namespace Launcher.Core.Lifecycle;

/// <summary>
/// The overall outcome of <see cref="IApplicationRuntime.StartAsync"/> (ADR-0026).
/// </summary>
public enum ApplicationStartStatus
{
    /// <summary>The runtime started and is idle, ready for activation.</summary>
    Started,

    /// <summary>Another instance of the same user owns the launcher, so this process should exit.</summary>
    AnotherInstanceRunning,
}
