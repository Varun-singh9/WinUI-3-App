using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Guards the documented dependency direction (TESTS.md: "Project references obey dependency direction").
/// </summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void Every_production_project_has_a_dependency_rule()
    {
        var graph = ProjectReferenceGraph.LoadFromRepository();

        Assert.Empty(ArchitectureChecks.FindProjectsWithoutRules(graph));
    }

    [Fact]
    public void Every_project_references_only_allowed_projects()
    {
        var graph = ProjectReferenceGraph.LoadFromRepository();

        Assert.Empty(ArchitectureChecks.FindDisallowedReferences(graph));
    }

    [Fact]
    public void Core_references_no_other_launcher_project()
    {
        var graph = ProjectReferenceGraph.LoadFromRepository();

        Assert.Empty(ArchitectureChecks.FindCoreReferences(graph));
    }

    [Fact]
    public void No_project_references_the_application_project()
    {
        var graph = ProjectReferenceGraph.LoadFromRepository();

        Assert.Empty(ArchitectureChecks.FindReferencesToApp(graph));
    }

    [Fact]
    public void Plugin_host_and_privileged_broker_do_not_reference_core()
    {
        var graph = ProjectReferenceGraph.LoadFromRepository();

        Assert.Empty(ArchitectureChecks.FindCoreReferencesFromIsolatedProcesses(graph));
    }

    [Fact]
    public void Reference_graph_has_no_cycles()
    {
        var graph = ProjectReferenceGraph.LoadFromRepository();

        Assert.Empty(graph.FindCycle());
    }
}
