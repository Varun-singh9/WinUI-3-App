namespace Launcher.Core.Lifecycle;

/// <summary>
/// How a wait for critical readiness ended (ADR-0026).
/// </summary>
public enum ReadinessStatus
{
    /// <summary>Every critical capability reported ready.</summary>
    Ready,

    /// <summary>A critical capability reported failure.</summary>
    Failed,

    /// <summary>The critical readiness timeout elapsed before every critical capability was ready.</summary>
    TimedOut,

    /// <summary>The caller cancelled the wait.</summary>
    Cancelled,
}
