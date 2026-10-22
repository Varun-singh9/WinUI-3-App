namespace Launcher.Core.Search;

/// <summary>
/// Provider health states (Implementation Specification section 12).
/// </summary>
public enum ProviderHealth
{
    /// <summary>The provider is working normally.</summary>
    Healthy,

    /// <summary>The provider is starting up and is not yet ready.</summary>
    Initializing,

    /// <summary>The provider works with reduced capability.</summary>
    Degraded,

    /// <summary>The provider has failed and is isolated.</summary>
    Failed,

    /// <summary>The provider is switched off.</summary>
    Disabled,
}
