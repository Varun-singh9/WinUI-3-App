namespace Launcher.Core.Search;

/// <summary>
/// One progressive update of the results for a search request.
/// </summary>
/// <param name="Version">The version of the request these results belong to.</param>
/// <param name="Results">The ranked, deduplicated results known so far.</param>
/// <param name="IsFinal"><see langword="true"/> when no further updates follow for this request.</param>
public sealed record SearchResultUpdate(long Version, IReadOnlyList<SearchResult> Results, bool IsFinal);
