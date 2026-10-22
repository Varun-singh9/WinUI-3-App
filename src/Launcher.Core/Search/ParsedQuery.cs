namespace Launcher.Core.Search;

/// <summary>
/// Structured result of parsing command-bar input. Prefix selection is state, not a string convention.
/// </summary>
/// <param name="RawInput">The input exactly as typed.</param>
/// <param name="Prefix">The recognized prefix, or <see langword="null"/> when none applies (including an unknown prefix that fell back to normal search).</param>
/// <param name="Command">The recognized command, or <see langword="null"/> when the input is not a command.</param>
/// <param name="SearchText">The literal search text left after the prefix and command are removed.</param>
public sealed record ParsedQuery(
    string RawInput,
    PrefixDefinition? Prefix,
    CommandInvocation? Command,
    string SearchText);
