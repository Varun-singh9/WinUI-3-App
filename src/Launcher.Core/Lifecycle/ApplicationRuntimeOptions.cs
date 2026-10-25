using Launcher.Core.Integrations;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// The minimal startup configuration the composition root passes to the runtime (ADR-0026). Persisting these values
/// through <c>IConfigurationService</c> is a later task.
/// </summary>
/// <param name="Hotkey">The global hotkey to register.</param>
/// <param name="CriticalReadinessTimeout">How long activation waits for critical readiness before the outcome is <see cref="ReadinessStatus.TimedOut"/>.</param>
public sealed record ApplicationRuntimeOptions(HotkeyGesture Hotkey, TimeSpan CriticalReadinessTimeout)
{
    /// <summary>
    /// Gets the default critical readiness timeout, five seconds. It is a default the project owner approved
    /// (ADR-0026), not a figure from the specification.
    /// </summary>
    public static TimeSpan DefaultCriticalReadinessTimeout { get; } = TimeSpan.FromSeconds(5);
}
