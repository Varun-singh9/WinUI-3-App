using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Guards the Phase 0 contract shapes against accidental change (TESTS.md: contract shapes match the specification).
/// </summary>
public sealed class ContractTests
{
    [Fact]
    public void Launcher_states_match_specification()
    {
        Assert.Empty(ContractChecks.LauncherStatesMatchSpecification());
    }

    [Fact]
    public void Activation_kinds_match_specification()
    {
        Assert.Empty(ContractChecks.ActivationKindsMatchSpecification());
    }

    [Fact]
    public void Provider_health_states_match_specification()
    {
        Assert.Empty(ContractChecks.ProviderHealthStatesMatchSpecification());
    }

    [Fact]
    public void Risk_levels_match_security_model()
    {
        Assert.Empty(ContractChecks.RiskLevelsMatchSecurityModel());
    }

    [Fact]
    public void Search_result_fields_match_specification()
    {
        Assert.Empty(ContractChecks.SearchResultFieldsMatchSpecification());
    }

    [Fact]
    public void Log_entry_fields_match_specification()
    {
        Assert.Empty(ContractChecks.LogEntryFieldsMatchSpecification());
    }

    [Fact]
    public void Plugin_manifest_fields_match_canonical_list()
    {
        Assert.Empty(ContractChecks.PluginManifestFieldsMatchCanonicalList());
    }

    [Fact]
    public void Ipc_message_fields_match_specification()
    {
        Assert.Empty(ContractChecks.IpcMessageFieldsMatchSpecification());
    }

    [Fact]
    public void Core_interfaces_exist()
    {
        Assert.Empty(ContractChecks.CoreInterfacesExist());
    }

    [Fact]
    public void Search_provider_contract_matches_specification()
    {
        Assert.Empty(ContractChecks.SearchProviderContractMatchesSpecification());
    }

    [Fact]
    public void Configuration_service_contract_matches_specification()
    {
        Assert.Empty(ContractChecks.ConfigurationServiceContractMatchesSpecification());
    }
}
