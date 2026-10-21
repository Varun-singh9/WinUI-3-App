using System.Xml.Linq;

namespace Launcher.Core.Tests;

/// <summary>
/// The project-to-project reference graph of the production projects under <c>src/</c>,
/// read directly from the .csproj files so it cannot drift from the real build.
/// </summary>
internal sealed class ProjectReferenceGraph
{
    private ProjectReferenceGraph(IReadOnlyDictionary<string, IReadOnlySet<string>> references)
    {
        References = references;
    }

    /// <summary>Gets each production project name mapped to the project names it references.</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> References { get; }

    /// <summary>Loads the graph from the repository that contains the running test assembly.</summary>
    public static ProjectReferenceGraph LoadFromRepository()
    {
        return LoadFrom(Path.Combine(FindRepositoryRoot(), "src"));
    }

    /// <summary>Loads the graph from every project folder directly under <paramref name="srcDirectory"/>.</summary>
    public static ProjectReferenceGraph LoadFrom(string srcDirectory)
    {
        var references = new SortedDictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);

        foreach (var projectDirectory in Directory.EnumerateDirectories(srcDirectory))
        {
            foreach (var projectFile in Directory.EnumerateFiles(projectDirectory, "*.csproj"))
            {
                var name = Path.GetFileNameWithoutExtension(projectFile);
                var referenced = XDocument.Load(projectFile)
                    .Descendants("ProjectReference")
                    .Select(element => (string?)element.Attribute("Include"))
                    .Where(include => !string.IsNullOrWhiteSpace(include))
                    .Select(include => Path.GetFileNameWithoutExtension(include!.Replace('\\', '/')))
                    .ToHashSet(StringComparer.Ordinal);

                references[name] = referenced;
            }
        }

        return new ProjectReferenceGraph(references);
    }

    /// <summary>Returns one dependency cycle as a list of project names, or an empty list when there is none.</summary>
    public IReadOnlyList<string> FindCycle()
    {
        var finished = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();

        foreach (var start in References.Keys)
        {
            var cycle = Visit(start, finished, path);
            if (cycle.Count > 0)
            {
                return cycle;
            }
        }

        return [];
    }

    private List<string> Visit(string project, HashSet<string> finished, List<string> path)
    {
        var index = path.IndexOf(project);
        if (index >= 0)
        {
            var cycle = path.Skip(index).ToList();
            cycle.Add(project);
            return cycle;
        }

        if (finished.Contains(project) || !References.TryGetValue(project, out var referenced))
        {
            return [];
        }

        path.Add(project);
        foreach (var next in referenced.OrderBy(name => name, StringComparer.Ordinal))
        {
            var cycle = Visit(next, finished, path);
            if (cycle.Count > 0)
            {
                return cycle;
            }
        }

        path.RemoveAt(path.Count - 1);
        finished.Add(project);
        return [];
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Launcher.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Launcher.sln was not found in any parent of the test output directory.");
    }
}
