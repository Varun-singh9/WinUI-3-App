using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Guards the hotkey and single-instance contract shapes added in Phase 1 against accidental change (ADR-0024).
/// </summary>
public sealed class IntegrationContractTests
{
    [Fact]
    public void Hotkey_modifiers_are_distinct_flags()
    {
        Assert.Empty(IntegrationContractChecks.HotkeyModifiersAreDistinctFlags());
    }

    [Fact]
    public void Hotkey_registration_statuses_match_adr_0024()
    {
        Assert.Empty(IntegrationContractChecks.HotkeyRegistrationStatusesMatchAdr0024());
    }

    [Fact]
    public void Hotkey_message_fields_match_adr_0024()
    {
        Assert.Empty(IntegrationContractChecks.HotkeyMessageFieldsMatchAdr0024());
    }

    [Fact]
    public void Hotkey_service_contract_matches_adr_0024()
    {
        Assert.Empty(IntegrationContractChecks.HotkeyServiceContractMatchesAdr0024());
    }

    [Fact]
    public void Single_instance_statuses_match_adr_0024()
    {
        Assert.Empty(IntegrationContractChecks.SingleInstanceStatusesMatchAdr0024());
    }

    [Fact]
    public void Single_instance_result_fields_match_adr_0024()
    {
        Assert.Empty(IntegrationContractChecks.SingleInstanceResultFieldsMatchAdr0024());
    }

    [Fact]
    public void Single_instance_owner_contract_matches_adr_0024()
    {
        Assert.Empty(IntegrationContractChecks.SingleInstanceOwnerContractMatchesAdr0024());
    }
}
