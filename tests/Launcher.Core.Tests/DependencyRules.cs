namespace Launcher.Core.Tests;

/// <summary>
/// The approved dependency direction between production projects (Implementation Specification
/// section 4.1). A project may reference only the projects listed for it here. Adding a project or
/// a reference is an architecture-boundary change: update this table together with an ADR.
/// </summary>
internal static class DependencyRules
{
    public const string Core = "Launcher.Core";
    public const string App = "Launcher.App";

    /// <summary>
    /// Allowed references per project. Entries marked provisional in PHASE_STATUS.md are ratified
    /// in the Phase 0 contracts step.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Allowed =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [Core] = Set(),
            ["Launcher.PluginContracts"] = Set(),
            ["Launcher.Diagnostics"] = Set(),
            ["Launcher.Security"] = Set(),
            ["Launcher.Configuration"] = Set(Core),
            ["Launcher.Data"] = Set(Core),
            ["Launcher.Search"] = Set(Core),
            ["Launcher.Providers"] = Set(Core, "Launcher.Search"),
            ["Launcher.Actions"] = Set(Core),
            ["Launcher.Integrations"] = Set(Core),
            ["Launcher.UI"] = Set(Core),
            ["Launcher.Motion"] = Set(Core),
            ["Launcher.PluginHost"] = Set("Launcher.PluginContracts", "Launcher.Security"),
            ["Launcher.PrivilegedBroker"] = Set("Launcher.Security"),

            // Composition root. The plugin host and privileged broker run as separate processes and are
            // never referenced by the application project.
            [App] = Set(
                Core, "Launcher.UI", "Launcher.Motion", "Launcher.Search", "Launcher.Providers",
                "Launcher.Actions", "Launcher.Integrations", "Launcher.Configuration", "Launcher.Data",
                "Launcher.Diagnostics", "Launcher.Security"),
        };

    private static HashSet<string> Set(params string[] names)
    {
        return new HashSet<string>(names, StringComparer.Ordinal);
    }
}
