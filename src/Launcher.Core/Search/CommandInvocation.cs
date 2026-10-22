namespace Launcher.Core.Search;

/// <summary>
/// A structured command with its arguments, kept separate from literal search text (Implementation Specification section 10).
/// </summary>
/// <param name="CommandId">The identifier of the command.</param>
/// <param name="Arguments">The parsed arguments, never joined back into a command line.</param>
public sealed record CommandInvocation(string CommandId, IReadOnlyList<string> Arguments);
