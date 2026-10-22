namespace Launcher.Core.Search;

/// <summary>
/// Runs a search across providers and streams progressive, version-tagged updates
/// (Implementation Specification section 11). Search routing, ranking and aggregation are implemented in Launcher.Search.
/// </summary>
public interface ISearchCoordinator
{
    /// <summary>Runs a search and streams updates until the final one.</summary>
    /// <param name="request">The request to run.</param>
    /// <param name="cancellationToken">Cancels the search.</param>
    /// <returns>A stream of updates whose last element has <see cref="SearchResultUpdate.IsFinal"/> set.</returns>
    IAsyncEnumerable<SearchResultUpdate> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
