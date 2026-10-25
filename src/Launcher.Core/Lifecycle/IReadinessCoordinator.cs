namespace Launcher.Core.Lifecycle;

/// <summary>
/// Tracks which capabilities are ready so activation can tell critical readiness from optional readiness
/// (roadmap Phase 1 "readiness coordinator", ADR-0026). Components report by name. Which names are critical is decided
/// by the composition root when the coordinator is created, not by this contract. Optional capabilities can be
/// reported too, but activation never waits for them, so an optional provider can never block it.
/// </summary>
public interface IReadinessCoordinator
{
    /// <summary>Gets a value indicating whether every critical capability is currently ready.</summary>
    bool IsCriticalReady { get; }

    /// <summary>
    /// Reports that a capability is ready. Reporting a capability that earlier reported failure clears that failure, so a
    /// component can recover and a later activation can succeed.
    /// </summary>
    /// <param name="capability">The capability name.</param>
    void ReportReady(string capability);

    /// <summary>Reports that a capability failed. A failed critical capability ends a wait with <see cref="ReadinessStatus.Failed"/>.</summary>
    /// <param name="capability">The capability name.</param>
    /// <param name="errorCode">A safe, stable error code.</param>
    void ReportFailed(string capability, string errorCode);

    /// <summary>
    /// Waits until every critical capability is ready, a critical capability fails, the configured timeout elapses, or
    /// the caller cancels. It returns immediately when critical readiness is already complete. It never throws for those
    /// outcomes.
    /// </summary>
    /// <param name="cancellationToken">Cancels the wait; the result then has status <see cref="ReadinessStatus.Cancelled"/>.</param>
    /// <returns>A task with the typed outcome.</returns>
    Task<ReadinessResult> WaitForCriticalAsync(CancellationToken cancellationToken);
}
