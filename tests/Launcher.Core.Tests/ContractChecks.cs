using System.Reflection;
using Launcher.Core.Actions;
using Launcher.Core.Configuration;
using Launcher.Core.Diagnostics;
using Launcher.Core.Lifecycle;
using Launcher.Core.Search;
using Launcher.PluginContracts;

namespace Launcher.Core.Tests;

/// <summary>
/// Contract-shape checks that pin the names and states fixed by the specification. Each public method returns the
/// list of mismatches, so an empty result means the contract matches. Changing a contract on purpose means changing
/// the expectation here together with an ADR.
/// </summary>
internal static class ContractChecks
{
    public static IReadOnlyList<string> LauncherStatesMatchSpecification()
    {
        return SameOrdered(
            nameof(LauncherState),
            ["Hidden", "Activating", "CriticalReady", "Morphing", "Ready", "PrefixOpen", "MenuOpen", "Searching", "Executing", "Closing", "ErrorRecovery"],
            Enum.GetNames<LauncherState>());
    }

    public static IReadOnlyList<string> ActivationKindsMatchSpecification()
    {
        return SameOrdered(nameof(ActivationKind), ["Cold", "Warm"], Enum.GetNames<ActivationKind>());
    }

    public static IReadOnlyList<string> ProviderHealthStatesMatchSpecification()
    {
        return SameOrdered(
            nameof(ProviderHealth),
            ["Healthy", "Initializing", "Degraded", "Failed", "Disabled"],
            Enum.GetNames<ProviderHealth>());
    }

    public static IReadOnlyList<string> RiskLevelsMatchSecurityModel()
    {
        return SameOrdered(
            nameof(RiskLevel),
            ["Safe", "Low", "Medium", "High", "Critical"],
            Enum.GetNames<RiskLevel>());
    }

    public static IReadOnlyList<string> SearchResultFieldsMatchSpecification()
    {
        return SameSet(
            nameof(SearchResult),
            ["Id", "Title", "Subtitle", "Icon", "ProviderId", "Score", "Categories", "Actions", "Metadata"],
            PropertyNames(typeof(SearchResult)));
    }

    public static IReadOnlyList<string> LogEntryFieldsMatchSpecification()
    {
        return SameSet(
            nameof(LogEntry),
            ["Timestamp", "Level", "Component", "EventId", "CorrelationId", "Duration", "ErrorCode", "Metadata"],
            PropertyNames(typeof(LogEntry)));
    }

    public static IReadOnlyList<string> PluginManifestFieldsMatchCanonicalList()
    {
        return SameSet(
            nameof(PluginManifest),
            ["Id", "Name", "Version", "ApiVersion", "ProtocolVersion", "Publisher", "Capabilities", "EntryPoint", "Providers", "Commands", "Settings", "MinimumLauncherVersion"],
            PropertyNames(typeof(PluginManifest)));
    }

    public static IReadOnlyList<string> IpcMessageFieldsMatchSpecification()
    {
        return [
            .. SameSet(nameof(IpcRequest), ["RequestId", "ProtocolVersion", "Operation", "Payload"], PropertyNames(typeof(IpcRequest))),
            .. SameSet(nameof(IpcResponse), ["RequestId", "Status", "ErrorCode", "Payload"], PropertyNames(typeof(IpcResponse))),
        ];
    }

    public static IReadOnlyList<string> CoreInterfacesExist()
    {
        string[] expected =
        [
            "IApplicationRuntime", "ILauncherLifecycle", "IActivationCoordinator", "ISearchCoordinator", "IProviderRegistry",
            "IActionExecutor", "IConfigurationService", "IDiagnostics", "IPluginManager", "IIntegrationManager",
        ];

        var coreAssembly = typeof(IApplicationRuntime).Assembly;
        var found = coreAssembly.GetExportedTypes().Where(type => type.IsInterface).Select(type => type.Name).ToHashSet(StringComparer.Ordinal);

        return expected
            .Where(name => !found.Contains(name))
            .Select(name => $"Public interface {name} is missing from Launcher.Core")
            .ToList();
    }

    public static IReadOnlyList<string> SearchProviderContractMatchesSpecification()
    {
        return [
            .. SameSet("ISearchProvider members", ["InitializeAsync", "SearchAsync", "GetHealth", "ShutdownAsync"], MethodNames(typeof(ISearchProvider))),
            .. SameSet("ISearchProvider properties", ["Info"], PropertyNames(typeof(ISearchProvider))),
        ];
    }

    public static IReadOnlyList<string> ConfigurationServiceContractMatchesSpecification()
    {
        return SameSet(
            "IConfigurationService members",
            ["Get", "Set", "Observe", "Validate", "ExportAsync", "ImportAsync", "MigrateAsync"],
            MethodNames(typeof(IConfigurationService)));
    }

    internal static IReadOnlyList<string> PropertyNames(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => property.Name).ToList();
    }

    internal static IReadOnlyList<string> MethodNames(Type type)
    {
        return type.GetMethods().Where(method => !method.IsSpecialName).Select(method => method.Name).Distinct(StringComparer.Ordinal).ToList();
    }

    internal static IReadOnlyList<string> SameOrdered(string subject, IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        return expected.SequenceEqual(actual, StringComparer.Ordinal)
            ? []
            : [$"{subject}: expected [{string.Join(", ", expected)}] in this order but found [{string.Join(", ", actual)}]"];
    }

    internal static IReadOnlyList<string> SameSet(string subject, IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        var missing = expected.Except(actual, StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToList();
        var unexpected = actual.Except(expected, StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToList();

        return missing.Count == 0 && unexpected.Count == 0
            ? []
            : [$"{subject}: missing [{string.Join(", ", missing)}], unexpected [{string.Join(", ", unexpected)}]"];
    }
}
