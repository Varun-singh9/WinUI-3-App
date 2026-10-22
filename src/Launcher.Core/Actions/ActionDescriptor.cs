namespace Launcher.Core.Actions;

/// <summary>
/// The display-only description of an action carried by a search result. Describing an action never executes it.
/// </summary>
/// <param name="ActionId">The identifier of the action.</param>
/// <param name="DisplayName">The user-visible name.</param>
/// <param name="Risk">The risk level that decides the confirmation requirement.</param>
/// <param name="Target">What the action acts on. Required for display when the risk is high or critical.</param>
/// <param name="Effect">What the action will do. Required for display when the risk is high or critical.</param>
public sealed record ActionDescriptor(
    string ActionId,
    string DisplayName,
    RiskLevel Risk,
    string? Target,
    string? Effect);
