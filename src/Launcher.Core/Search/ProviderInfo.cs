namespace Launcher.Core.Search;

/// <summary>
/// What a provider declares about itself (Project Overview section 8). The cost scale and lifecycle modes are provisional.
/// </summary>
/// <param name="Id">The stable provider identifier.</param>
/// <param name="DisplayName">The user-visible name.</param>
/// <param name="StartupCost">The declared startup cost.</param>
/// <param name="QueryCost">The declared per-query cost.</param>
/// <param name="MemoryCost">The declared memory cost.</param>
/// <param name="CpuCost">The declared CPU cost.</param>
/// <param name="UsesNetwork">Whether the provider uses the network. Network access is denied unless a provider requires it.</param>
/// <param name="RequiresPersistence">Whether the provider needs persistent storage.</param>
/// <param name="Permissions">The capability identifiers the provider needs. Default deny applies.</param>
/// <param name="LifecycleMode">When the provider is initialized.</param>
public sealed record ProviderInfo(
    string Id,
    string DisplayName,
    ResourceCost StartupCost,
    ResourceCost QueryCost,
    ResourceCost MemoryCost,
    ResourceCost CpuCost,
    bool UsesNetwork,
    bool RequiresPersistence,
    IReadOnlyList<string> Permissions,
    ProviderLifecycleMode LifecycleMode);
