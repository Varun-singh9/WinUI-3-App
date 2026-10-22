namespace Launcher.Core.Integrations;

/// <summary>
/// Owns Windows integrations behind interfaces so the rest of the application is not coupled to OS APIs.
/// Every integration detects its capability and has a fallback path (Implementation Specification section 14).
/// </summary>
public interface IIntegrationManager
{
    /// <summary>Initializes integrations. Never blocks critical activation.</summary>
    /// <param name="cancellationToken">Cancels initialization.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Reports whether an integration is available on this machine.</summary>
    /// <param name="integrationId">The integration identifier.</param>
    /// <returns><see langword="true"/> when the integration is available; otherwise <see langword="false"/>.</returns>
    bool IsAvailable(string integrationId);
}
