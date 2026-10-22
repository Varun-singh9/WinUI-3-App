namespace Launcher.Core.Search;

/// <summary>
/// When a provider is initialized. The set of modes is provisional (Implementation Specification section 12 asks for lazy or asynchronous initialization).
/// </summary>
public enum ProviderLifecycleMode
{
    /// <summary>Initialized in the background after the critical runtime is ready.</summary>
    Background,

    /// <summary>Initialized on first use.</summary>
    Lazy,
}
