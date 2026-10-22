namespace Launcher.Core.Actions;

/// <summary>
/// Structured success or failure information returned by an action.
/// </summary>
/// <param name="Succeeded">Whether the action completed successfully.</param>
/// <param name="ErrorCode">A safe, stable error code when the action failed.</param>
/// <param name="Message">A safe message for the user.</param>
public sealed record ActionResult(bool Succeeded, string? ErrorCode, string? Message);
