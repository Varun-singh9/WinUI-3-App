namespace Launcher.Core.Actions;

/// <summary>
/// The outcome of validating an action's arguments before execution.
/// </summary>
/// <param name="IsValid">Whether the arguments are acceptable.</param>
/// <param name="ErrorCode">A safe, stable error code when the arguments are not acceptable.</param>
/// <param name="Message">A safe message for the user when the arguments are not acceptable.</param>
public sealed record ActionValidation(bool IsValid, string? ErrorCode, string? Message);
