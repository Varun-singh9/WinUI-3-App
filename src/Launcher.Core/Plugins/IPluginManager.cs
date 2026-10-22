using Launcher.PluginContracts;

namespace Launcher.Core.Plugins;

/// <summary>
/// Discovers and tracks out-of-process plugins. A plugin never inherits Core privileges and cannot expand its own
/// permissions (ADR-0002, ADR-0005). The detailed lifecycle is defined in the plugin phase.
/// </summary>
public interface IPluginManager
{
    /// <summary>Gets the manifests of the plugins that were discovered and accepted.</summary>
    IReadOnlyList<PluginManifest> Plugins { get; }

    /// <summary>Discovers plugins and validates their manifests.</summary>
    /// <param name="cancellationToken">Cancels discovery.</param>
    /// <returns>A task that completes when discovery has finished.</returns>
    Task DiscoverAsync(CancellationToken cancellationToken);
}
