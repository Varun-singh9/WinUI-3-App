namespace Launcher.Core.Lifecycle;

/// <summary>
/// The typed outcome of waiting for critical readiness. A failure, a timeout and a cancellation are values, not
/// exceptions, so activation can move to a recoverable state instead of crashing.
/// </summary>
/// <param name="Status">How the wait ended.</param>
/// <param name="Capability">The critical capability that failed or was still pending, or <see langword="null"/> when the status is <see cref="ReadinessStatus.Ready"/> or <see cref="ReadinessStatus.Cancelled"/>.</param>
/// <param name="ErrorCode">A safe, stable error code for a failure, or <see langword="null"/> otherwise.</param>
public sealed record ReadinessResult(ReadinessStatus Status, string? Capability, string? ErrorCode);
