using Launcher.Core.Lifecycle;

namespace Launcher.Core.Tests;

/// <summary>
/// Contract-shape checks for the lifecycle contracts added in Phase 1 Task 6 (ADR-0026). Like
/// <see cref="ContractChecks"/>, each method returns the list of mismatches, so an empty result means the contract
/// matches. Changing these contracts on purpose means changing the expectation here together with an ADR.
/// </summary>
internal static class LifecycleContractChecks
{
    public static IReadOnlyList<string> ReadinessStatusesMatchAdr0026()
    {
        return ContractChecks.SameOrdered(
            nameof(ReadinessStatus),
            ["Ready", "Failed", "TimedOut", "Cancelled"],
            Enum.GetNames<ReadinessStatus>());
    }

    public static IReadOnlyList<string> ReadinessResultFieldsMatchAdr0026()
    {
        return ContractChecks.SameSet(
            nameof(ReadinessResult),
            ["Status", "Capability", "ErrorCode"],
            ContractChecks.PropertyNames(typeof(ReadinessResult)));
    }

    public static IReadOnlyList<string> ReadinessCoordinatorContractMatchesAdr0026()
    {
        var type = typeof(IReadinessCoordinator);
        return
        [
            .. ContractChecks.SameSet("IReadinessCoordinator methods", ["ReportReady", "ReportFailed", "WaitForCriticalAsync"], ContractChecks.MethodNames(type)),
            .. ContractChecks.SameSet("IReadinessCoordinator properties", ["IsCriticalReady"], ContractChecks.PropertyNames(type)),
        ];
    }

    public static IReadOnlyList<string> RuntimeOptionsMatchAdr0026()
    {
        List<string> problems =
        [
            .. ContractChecks.SameSet(
                nameof(ApplicationRuntimeOptions),
                ["Hotkey", "CriticalReadinessTimeout"],
                ContractChecks.PropertyNames(typeof(ApplicationRuntimeOptions))),
        ];

        if (ApplicationRuntimeOptions.DefaultCriticalReadinessTimeout != TimeSpan.FromSeconds(5))
        {
            problems.Add($"{nameof(ApplicationRuntimeOptions)}.DefaultCriticalReadinessTimeout: expected 5 seconds (owner-approved default)");
        }

        return problems;
    }

    public static IReadOnlyList<string> StartStatusesMatchAdr0026()
    {
        return ContractChecks.SameOrdered(
            nameof(ApplicationStartStatus),
            ["Started", "AnotherInstanceRunning"],
            Enum.GetNames<ApplicationStartStatus>());
    }

    public static IReadOnlyList<string> StartResultFieldsMatchAdr0026()
    {
        return ContractChecks.SameSet(
            nameof(ApplicationStartResult),
            ["Status", "SingleInstance", "OwnerSignaled", "Hotkey"],
            ContractChecks.PropertyNames(typeof(ApplicationStartResult)));
    }

    public static IReadOnlyList<string> RuntimeContractMatchesAdr0026()
    {
        var type = typeof(IApplicationRuntime);
        List<string> problems =
        [
            .. ContractChecks.SameSet("IApplicationRuntime methods", ["StartAsync", "ShutdownAsync", "RestartAsync"], ContractChecks.MethodNames(type)),
            .. ContractChecks.SameSet("IApplicationRuntime properties", ["Lifecycle"], ContractChecks.PropertyNames(type)),
        ];

        var startReturnType = type.GetMethod(nameof(IApplicationRuntime.StartAsync))?.ReturnType;
        if (startReturnType != typeof(Task<ApplicationStartResult>))
        {
            problems.Add($"IApplicationRuntime.StartAsync: expected the return type Task<ApplicationStartResult> but found {startReturnType}");
        }

        return problems;
    }
}
