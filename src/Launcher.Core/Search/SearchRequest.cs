namespace Launcher.Core.Search;

/// <summary>
/// One search operation. Every request carries a version so a stale request can never overwrite newer results
/// (Implementation Specification section 11).
/// </summary>
/// <param name="Version">The request/version identifier. Higher values are newer.</param>
/// <param name="Query">The parsed query to run.</param>
public sealed record SearchRequest(long Version, ParsedQuery Query);
