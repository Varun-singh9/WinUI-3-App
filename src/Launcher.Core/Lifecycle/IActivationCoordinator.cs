namespace Launcher.Core.Lifecycle;

/// <summary>
/// Coordinates cold and warm activation. Optional providers never block activation (Implementation Specification section 7).
/// </summary>
public interface IActivationCoordinator
{
    /// <summary>Activates the launcher and makes the command bar usable.</summary>
    /// <param name="cancellationToken">Cancels the activation.</param>
    /// <returns>A task that completes when the command bar is usable.</returns>
    Task ActivateAsync(CancellationToken cancellationToken);

    /// <summary>Deactivates the launcher.</summary>
    /// <param name="cancellationToken">Cancels the deactivation.</param>
    /// <returns>A task that completes when the launcher is hidden.</returns>
    Task DeactivateAsync(CancellationToken cancellationToken);
}
