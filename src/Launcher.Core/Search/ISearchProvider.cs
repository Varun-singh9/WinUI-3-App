namespace Launcher.Core.Search;

/// <summary>
/// The narrow contract every search provider implements (Implementation Specification section 12).
/// A provider timeout must not hang a search and a provider crash must not crash the launcher.
/// </summary>
public interface ISearchProvider
{
    /// <summary>Gets what the provider declares about itself.</summary>
    ProviderInfo Info { get; }

    /// <summary>Initializes the provider. Never called on the critical activation path.</summary>
    /// <param name="cancellationToken">Cancels initialization.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Runs one search.</summary>
    /// <param name="request">The request, including its version.</param>
    /// <param name="cancellationToken">Cancels the search.</param>
    /// <returns>The provider's results, still subject to validation and global ranking.</returns>
    Task<IReadOnlyList<SearchResult>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);

    /// <summary>Gets the current health of the provider.</summary>
    /// <returns>The provider's health state.</returns>
    ProviderHealth GetHealth();

    /// <summary>Shuts the provider down.</summary>
    /// <param name="cancellationToken">Cancels the wait for shutdown to finish.</param>
    /// <returns>A task that completes when the provider has shut down.</returns>
    Task ShutdownAsync(CancellationToken cancellationToken);
}
