using System.Reflection;
using Launcher.Core.Integrations;

namespace Launcher.Core.Tests;

/// <summary>
/// Contract-shape checks for the hotkey and single-instance contracts added in Phase 1 (ADR-0024). Like
/// <see cref="ContractChecks"/>, each method returns the list of mismatches, so an empty result means the contract
/// matches. Changing these contracts on purpose means changing the expectation here together with an ADR.
/// </summary>
internal static class IntegrationContractChecks
{
    public static IReadOnlyList<string> HotkeyModifiersAreDistinctFlags()
    {
        List<string> problems =
        [
            .. ContractChecks.SameOrdered(
                nameof(HotkeyModifiers),
                ["None", "Alt", "Control", "Shift", "Windows"],
                Enum.GetNames<HotkeyModifiers>()),
        ];

        if (!typeof(HotkeyModifiers).IsDefined(typeof(FlagsAttribute), inherit: false))
        {
            problems.Add($"{nameof(HotkeyModifiers)}: expected the [Flags] attribute");
        }

        var values = Enum.GetValues<HotkeyModifiers>().Where(value => value != HotkeyModifiers.None).Select(value => (int)value).ToList();
        if (values.Any(value => (value & (value - 1)) != 0) || values.Distinct().Count() != values.Count)
        {
            problems.Add($"{nameof(HotkeyModifiers)}: expected every modifier to be a distinct single bit");
        }

        return problems;
    }

    public static IReadOnlyList<string> HotkeyRegistrationStatusesMatchAdr0024()
    {
        return ContractChecks.SameOrdered(
            nameof(HotkeyRegistrationStatus),
            ["Registered", "InUse", "InvalidGesture", "Failed"],
            Enum.GetNames<HotkeyRegistrationStatus>());
    }

    public static IReadOnlyList<string> HotkeyMessageFieldsMatchAdr0024()
    {
        return
        [
            .. ContractChecks.SameSet(nameof(HotkeyGesture), ["Modifiers", "VirtualKey"], ContractChecks.PropertyNames(typeof(HotkeyGesture))),
            .. ContractChecks.SameSet(nameof(HotkeyRegistrationResult), ["Status", "Gesture", "ErrorCode"], ContractChecks.PropertyNames(typeof(HotkeyRegistrationResult))),
            .. ContractChecks.SameSet(nameof(HotkeyPressedEventArgs), ["Gesture", "Timestamp"], ContractChecks.PropertyNames(typeof(HotkeyPressedEventArgs))),
        ];
    }

    public static IReadOnlyList<string> HotkeyServiceContractMatchesAdr0024()
    {
        var type = typeof(IHotkeyService);
        return
        [
            .. ContractChecks.SameSet("IHotkeyService methods", ["RegisterAsync", "UnregisterAsync"], ContractChecks.MethodNames(type)),
            .. ContractChecks.SameSet("IHotkeyService properties", ["Current"], ContractChecks.PropertyNames(type)),
            .. ContractChecks.SameSet("IHotkeyService events", ["Pressed"], EventNames(type)),
        ];
    }

    public static IReadOnlyList<string> SingleInstanceStatusesMatchAdr0024()
    {
        return ContractChecks.SameOrdered(
            nameof(SingleInstanceStatus),
            ["Owner", "OtherInstanceRunning", "Failed"],
            Enum.GetNames<SingleInstanceStatus>());
    }

    public static IReadOnlyList<string> SingleInstanceResultFieldsMatchAdr0024()
    {
        return ContractChecks.SameSet(
            nameof(SingleInstanceResult),
            ["Status", "ErrorCode"],
            ContractChecks.PropertyNames(typeof(SingleInstanceResult)));
    }

    public static IReadOnlyList<string> SingleInstanceOwnerContractMatchesAdr0024()
    {
        var type = typeof(ISingleInstanceOwner);
        return
        [
            .. ContractChecks.SameSet("ISingleInstanceOwner methods", ["TryAcquire", "TrySignalOwner", "Release"], ContractChecks.MethodNames(type)),
            .. ContractChecks.SameSet("ISingleInstanceOwner properties", ["IsOwner"], ContractChecks.PropertyNames(type)),
            .. ContractChecks.SameSet("ISingleInstanceOwner events", ["ActivationRequested"], EventNames(type)),
        ];
    }

    private static IReadOnlyList<string> EventNames(Type type)
    {
        return type.GetEvents(BindingFlags.Public | BindingFlags.Instance).Select(eventInfo => eventInfo.Name).ToList();
    }
}
