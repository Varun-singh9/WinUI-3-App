namespace Launcher.Core.Integrations;

/// <summary>
/// The typed outcome of trying to acquire single-instance ownership. Failure is a value, not an exception.
/// </summary>
/// <param name="Status">What happened.</param>
/// <param name="ErrorCode">A safe, stable error code when the status is <see cref="SingleInstanceStatus.Failed"/>; otherwise <see langword="null"/>.</param>
public sealed record SingleInstanceResult(SingleInstanceStatus Status, string? ErrorCode);
