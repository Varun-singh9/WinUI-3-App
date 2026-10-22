namespace Launcher.Core.Actions;

/// <summary>
/// The structured input for validating and executing one action. Arguments are structured values and are never
/// turned into an unrestricted command line.
/// </summary>
/// <param name="ActionId">The action to run.</param>
/// <param name="ResultId">The search result the action was chosen from, if any.</param>
/// <param name="Arguments">The named arguments to validate before execution.</param>
public sealed record ActionContext(
    string ActionId,
    string? ResultId,
    IReadOnlyDictionary<string, string> Arguments);
