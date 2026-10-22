namespace Launcher.Core.Lifecycle;

/// <summary>
/// Distinguishes the two activation paths so diagnostics can tell them apart (Implementation Specification section 7).
/// </summary>
public enum ActivationKind
{
    /// <summary>First activation: the runtime and window state must be created.</summary>
    Cold,

    /// <summary>Later activation that reuses existing runtime and window state.</summary>
    Warm,
}
