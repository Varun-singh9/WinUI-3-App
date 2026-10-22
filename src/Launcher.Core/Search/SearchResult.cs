using Launcher.Core.Actions;

namespace Launcher.Core.Search;

/// <summary>
/// A normalized search result. Results describe actions; the action executor performs them
/// (Implementation Specification sections 11 and 13).
/// </summary>
/// <param name="Id">The result identifier, unique within its provider.</param>
/// <param name="Title">The primary text.</param>
/// <param name="Subtitle">The secondary text, if any.</param>
/// <param name="Icon">A semantic icon token that the UI resolves, if any.</param>
/// <param name="ProviderId">The provider that produced the result.</param>
/// <param name="Score">The provider-normalized score used by global ranking.</param>
/// <param name="Categories">The categories the result belongs to.</param>
/// <param name="Actions">The actions available for the result.</param>
/// <param name="Metadata">Extra provider data. Must not contain secrets.</param>
public sealed record SearchResult(
    string Id,
    string Title,
    string? Subtitle,
    string? Icon,
    string ProviderId,
    double Score,
    IReadOnlyList<string> Categories,
    IReadOnlyList<ActionDescriptor> Actions,
    IReadOnlyDictionary<string, string> Metadata);
