namespace Launcher.Core.Search;

/// <summary>
/// Coarse cost declaration used in <see cref="ProviderInfo"/>. The scale is provisional until the first providers define real budgets.
/// </summary>
public enum ResourceCost
{
    /// <summary>Negligible cost.</summary>
    Low,

    /// <summary>Noticeable but acceptable cost.</summary>
    Medium,

    /// <summary>Significant cost that the scheduler must account for.</summary>
    High,
}
