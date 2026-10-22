namespace Launcher.Core.Lifecycle;

/// <summary>
/// The single runtime coordinator that owns orchestration and lifecycle (Implementation Specification section 5).
/// It does not contain feature implementations.
/// </summary>
public interface IApplicationRuntime
{
    /// <summary>Gets the semantic lifecycle exposed to the UI.</summary>
    ILauncherLifecycle Lifecycle { get; }

    /// <summary>Starts the runtime: configuration, diagnostics, single-instance ownership and critical resources.</summary>
    /// <param name="cancellationToken">Cancels startup.</param>
    /// <returns>A task that completes when the runtime is ready for activation.</returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Shuts the runtime down in a controlled way.</summary>
    /// <param name="cancellationToken">Cancels the wait for shutdown to finish.</param>
    /// <returns>A task that completes when shutdown has finished.</returns>
    Task ShutdownAsync(CancellationToken cancellationToken);

    /// <summary>Restarts the runtime in a controlled way.</summary>
    /// <param name="cancellationToken">Cancels the wait for the restart to finish.</param>
    /// <returns>A task that completes when the runtime is ready again.</returns>
    Task RestartAsync(CancellationToken cancellationToken);
}
