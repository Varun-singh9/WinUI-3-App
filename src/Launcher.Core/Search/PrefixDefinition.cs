namespace Launcher.Core.Search;

/// <summary>
/// One command-bar prefix. Prefixes are routing and performance hints, not only visual labels, and carry text,
/// symbol, icon and color together (Project Overview section 7).
/// </summary>
/// <param name="Id">The stable identifier of the prefix.</param>
/// <param name="DisplayName">The user-visible name.</param>
/// <param name="TextAliases">Text aliases that select the prefix, for example a short word followed by a colon.</param>
/// <param name="SymbolAliases">Symbol aliases that select the prefix. Text and symbol aliases resolve identically.</param>
/// <param name="Icon">A semantic icon token that the UI resolves to an actual icon.</param>
/// <param name="Color">A semantic color token. Color is never the only carrier of meaning.</param>
/// <param name="ProviderIds">The providers this prefix routes the query to.</param>
public sealed record PrefixDefinition(
    string Id,
    string DisplayName,
    IReadOnlyList<string> TextAliases,
    IReadOnlyList<string> SymbolAliases,
    string Icon,
    string Color,
    IReadOnlyList<string> ProviderIds);
