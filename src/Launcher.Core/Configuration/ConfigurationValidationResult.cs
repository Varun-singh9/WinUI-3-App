namespace Launcher.Core.Configuration;

/// <summary>
/// The outcome of validating configuration.
/// </summary>
/// <param name="IsValid">Whether the configuration is valid.</param>
/// <param name="Errors">Safe descriptions of what is invalid. Must not contain secrets.</param>
public sealed record ConfigurationValidationResult(bool IsValid, IReadOnlyList<string> Errors);
