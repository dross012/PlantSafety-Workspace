// AUTHOR: Security & Compliance Agent (Phase 7 §5.3)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for DataResidencyEnforcer -- verifies the real enforcement rule
//   comparing a tenant's configured DataResidencyPolicy against the platform's current
//   deployment region, using real (non-mocked) collaborator implementations.
// SchemaVersion: 1

using Microsoft.Extensions.Options;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Domain.MultiRegion.DataResidency;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using PlantSafety.SharedKernel.Identifiers;
using Xunit;

namespace PlantSafety.Tests.Application.Enterprise.MultiRegion;

/// <summary>Tests for <see cref="DataResidencyEnforcer"/>.</summary>
public sealed class DataResidencyEnforcerTests
{
    private static DataResidencyEnforcer CreateSut(
        Region currentRegion,
        IReadOnlyDictionary<string, string>? tenantPolicies = null)
    {
        var regionProvider = new ConfiguredCurrentDeploymentRegionProvider(
            Options.Create(new CurrentDeploymentRegionOptions { RegionCode = currentRegion.Code }));

        var profileStore = new ConfiguredTenantDataResidencyProfileStore(
            Options.Create(new TenantDataResidencyOptions { Policies = tenantPolicies ?? new Dictionary<string, string>() }));

        return new DataResidencyEnforcer(profileStore, regionProvider);
    }

    [Fact]
    public void IsSatisfied_TenantWithNoConfiguredPolicy_DefaultsToGlobal_AlwaysSatisfied()
    {
        var sut = CreateSut(Region.Eu);

        Assert.True(sut.IsSatisfied(Guid.NewGuid()));
    }

    [Fact]
    public void IsSatisfied_EuOnlyTenant_DeployedInUsEast_IsNotSatisfied()
    {
        var tenantId = Guid.NewGuid();
        var sut = CreateSut(
            currentRegion: Region.UsEast,
            tenantPolicies: new Dictionary<string, string> { [tenantId.ToString("D")] = "EuOnly" });

        Assert.False(sut.IsSatisfied(tenantId));
    }

    [Fact]
    public void IsSatisfied_EuOnlyTenant_DeployedInEu_IsSatisfied()
    {
        var tenantId = Guid.NewGuid();
        var sut = CreateSut(
            currentRegion: Region.Eu,
            tenantPolicies: new Dictionary<string, string> { [tenantId.ToString("D")] = "EuOnly" });

        Assert.True(sut.IsSatisfied(tenantId));
    }

    [Fact]
    public void EnsureSatisfied_ViolatingTenant_ThrowsDataResidencyViolationExceptionWithDetails()
    {
        var tenantId = Guid.NewGuid();
        var sut = CreateSut(
            currentRegion: Region.UsWest,
            tenantPolicies: new Dictionary<string, string> { [tenantId.ToString("D")] = "EuOnly" });

        var correlationId = new CorrelationId(Guid.NewGuid());
        var causationId = new CausationId(Guid.NewGuid());

        var ex = Assert.Throws<DataResidencyViolationException>(
            () => sut.EnsureSatisfied(tenantId, correlationId, causationId));

        Assert.Equal(tenantId, ex.TenantId);
        Assert.Same(DataResidencyPolicy.EuOnly, ex.RequiredPolicy);
        Assert.Same(Region.UsWest, ex.CurrentDeploymentRegion);
    }

    [Fact]
    public void EnsureSatisfied_CompliantTenant_DoesNotThrow()
    {
        var tenantId = Guid.NewGuid();
        var sut = CreateSut(
            currentRegion: Region.UsEast,
            tenantPolicies: new Dictionary<string, string> { [tenantId.ToString("D")] = "UsOnly" });

        var exception = Record.Exception(() =>
            sut.EnsureSatisfied(tenantId, new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid())));

        Assert.Null(exception);
    }

    [Fact]
    public void ConfiguredCurrentDeploymentRegionProvider_UnconfiguredRegionCode_FallsBackToDefault()
    {
        var provider = new ConfiguredCurrentDeploymentRegionProvider(
            Options.Create(new CurrentDeploymentRegionOptions { RegionCode = null }));

        Assert.Same(Region.Default, provider.CurrentRegion);
    }
}
