namespace Launcher.Core.Actions;

/// <summary>
/// Risk levels from the Security Model. SAFE and LOW run after ordinary validation. MEDIUM requires explicit confirmation.
/// HIGH and CRITICAL require explicit confirmation that presents target and effect.
/// </summary>
public enum RiskLevel
{
    /// <summary>Read-only or low-impact.</summary>
    Safe,

    /// <summary>A reversible user action.</summary>
    Low,

    /// <summary>A meaningful state or process change.</summary>
    Medium,

    /// <summary>Destructive, broad or privileged effect.</summary>
    High,

    /// <summary>Potentially system-wide or security-sensitive.</summary>
    Critical,
}
