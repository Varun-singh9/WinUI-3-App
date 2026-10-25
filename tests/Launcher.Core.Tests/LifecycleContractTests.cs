using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Guards the lifecycle contract shapes added in Phase 1 Task 6 against accidental change (ADR-0026).
/// </summary>
public sealed class LifecycleContractTests
{
    [Fact]
    public void Readiness_statuses_match_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.ReadinessStatusesMatchAdr0026());
    }

    [Fact]
    public void Readiness_result_fields_match_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.ReadinessResultFieldsMatchAdr0026());
    }

    [Fact]
    public void Readiness_coordinator_contract_matches_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.ReadinessCoordinatorContractMatchesAdr0026());
    }

    [Fact]
    public void Runtime_options_match_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.RuntimeOptionsMatchAdr0026());
    }

    [Fact]
    public void Start_statuses_match_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.StartStatusesMatchAdr0026());
    }

    [Fact]
    public void Start_result_fields_match_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.StartResultFieldsMatchAdr0026());
    }

    [Fact]
    public void Runtime_contract_matches_adr_0026()
    {
        Assert.Empty(LifecycleContractChecks.RuntimeContractMatchesAdr0026());
    }
}
