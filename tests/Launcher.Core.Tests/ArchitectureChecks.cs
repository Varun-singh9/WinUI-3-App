namespace Launcher.Core.Tests;

/// <summary>
/// Dependency-direction checks that return human-readable violations instead of asserting, so the
/// same logic backs the unit tests and any ad-hoc verification. An empty result means the rule holds.
/// </summary>
internal static class ArchitectureChecks
{
    /// <summary>Production projects that exist in the graph but have no entry in <see cref="DependencyRules.Allowed"/>.</summary>
    public static IReadOnlyList<string> FindProjectsWithoutRules(ProjectReferenceGraph graph)
    {
        return graph.References.Keys
            .Where(project => !DependencyRules.Allowed.ContainsKey(project))
            .Select(project => $"{project} has no entry in DependencyRules.Allowed")
            .ToList();
    }

    /// <summary>References that a project is not allowed to make.</summary>
    public static IReadOnlyList<string> FindDisallowedReferences(ProjectReferenceGraph graph)
    {
        var violations = new List<string>();

        foreach (var (project, referenced) in graph.References)
        {
            if (!DependencyRules.Allowed.TryGetValue(project, out var allowed))
            {
                continue;
            }

            foreach (var reference in referenced.Where(name => !allowed.Contains(name)).OrderBy(name => name, StringComparer.Ordinal))
            {
                violations.Add($"{project} -> {reference} is not an allowed reference");
            }
        }

        return violations;
    }

    /// <summary>Core may reference only the leaf PluginContracts project: Core coordinates and holds no feature code (ADR-0019).</summary>
    public static IReadOnlyList<string> FindUnexpectedCoreReferences(ProjectReferenceGraph graph)
    {
        return graph.References.TryGetValue(DependencyRules.Core, out var referenced)
            ? referenced
                .Where(name => name != DependencyRules.PluginContracts)
                .Select(name => $"{DependencyRules.Core} -> {name}")
                .OrderBy(text => text, StringComparer.Ordinal)
                .ToList()
            : [$"{DependencyRules.Core} was not found in the graph"];
    }

    /// <summary>Leaf projects (Security and PluginContracts) must not reference any other Launcher project.</summary>
    public static IReadOnlyList<string> FindLeafProjectReferences(ProjectReferenceGraph graph)
    {
        return new[] { DependencyRules.Security, DependencyRules.PluginContracts }
            .SelectMany(leaf => graph.References.TryGetValue(leaf, out var referenced)
                ? referenced.Select(name => $"{leaf} -> {name}")
                : [$"{leaf} was not found in the graph"])
            .ToList();
    }

    /// <summary>Projects that reference the application project, which is only ever an entry point.</summary>
    public static IReadOnlyList<string> FindReferencesToApp(ProjectReferenceGraph graph)
    {
        return graph.References
            .Where(pair => pair.Value.Contains(DependencyRules.App))
            .Select(pair => $"{pair.Key} -> {DependencyRules.App}")
            .ToList();
    }

    /// <summary>The out-of-process plugin host and privileged broker must not pull in Core.</summary>
    public static IReadOnlyList<string> FindCoreReferencesFromIsolatedProcesses(ProjectReferenceGraph graph)
    {
        return new[] { "Launcher.PluginHost", "Launcher.PrivilegedBroker" }
            .Where(project => graph.References.TryGetValue(project, out var referenced) && referenced.Contains(DependencyRules.Core))
            .Select(project => $"{project} -> {DependencyRules.Core}")
            .ToList();
    }
}
