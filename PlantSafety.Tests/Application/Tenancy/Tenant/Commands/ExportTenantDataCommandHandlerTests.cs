// AUTHOR: PlantSafety Platform Team (Phase 7 §5.3 Data Residency wiring)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for ExportTenantDataCommandHandler, focused on the Phase 7 §5.3
//   Data Residency enforcement point: an export must be rejected -- loudly, not silently -- when
//   the target tenant's configured DataResidencyPolicy does not permit the platform's current
//   deployment region, and must proceed normally when it does.
// SchemaVersion: 1

using Microsoft.Extensions.Options;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;
using PlantSafety.Application.Tenancy.Tenant.Commands;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using DomainTenant = PlantSafety.Domain.Tenancy.Tenant.Aggregates.Tenant;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Tests.Application.Tenancy.Tenant.Commands;

/// <summary>Tests for <see cref="ExportTenantDataCommandHandler"/>.</summary>
public sealed class ExportTenantDataCommandHandlerTests : UnitTestBase
{
    private static DomainTenant CreateProvisionedTenant() =>
        DomainTenant.Provision(
            "Acme Manufacturing", "UsEast", SubscriptionPlanCode.Standard,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

    private static IDataResidencyEnforcer CreateEnforcer(
        Region currentRegion, IReadOnlyDictionary<string, string>? tenantPolicies = null)
    {
        var regionProvider = new ConfiguredCurrentDeploymentRegionProvider(
            Options.Create(new CurrentDeploymentRegionOptions { RegionCode = currentRegion.Code }));

        var profileStore = new ConfiguredTenantDataResidencyProfileStore(
            Options.Create(new TenantDataResidencyOptions { Policies = tenantPolicies ?? new Dictionary<string, string>() }));

        return new DataResidencyEnforcer(profileStore, regionProvider);
    }

    private static ExportTenantDataCommandHandler CreateSut(
        DomainTenant tenant, IDataResidencyEnforcer enforcer, out ITenantDataExportStore exportStore)
    {
        var repository = Substitute.For<ITenantRepository>();
        repository.GetByIdAsync(tenant.TenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        exportStore = Substitute.For<ITenantDataExportStore>();
        exportStore.SaveAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns("blob://tenant-exports/fake-reference.json");

        return new ExportTenantDataCommandHandler(repository, exportStore, enforcer, NullLogger<ExportTenantDataCommandHandler>());
    }

    [Fact]
    public async Task Handle_TenantResidencyPolicyViolatedByCurrentRegion_RejectsExportWithoutWritingArtifact()
    {
        var tenant = CreateProvisionedTenant();
        var enforcer = CreateEnforcer(
            currentRegion: Region.UsEast,
            tenantPolicies: new Dictionary<string, string> { [tenant.TenantId.Value.ToString("D")] = "EuOnly" });
        var sut = CreateSut(tenant, enforcer, out var exportStore);

        var command = ExportTenantDataCommand.FromDto(tenant.TenantId.Value, TestUserId.Value, TestCorrelationId.Value, TestCausationId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("an EU-only tenant's export must never be written from a US deployment region");
        result.Error!.ErrorCode.Should().Be("Tenancy.Tenant.ExportResidencyViolation");
        await exportStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantResidencyPolicySatisfiedByCurrentRegion_ExportsSuccessfully()
    {
        var tenant = CreateProvisionedTenant();
        var enforcer = CreateEnforcer(
            currentRegion: Region.UsEast,
            tenantPolicies: new Dictionary<string, string> { [tenant.TenantId.Value.ToString("D")] = "UsOnly" });
        var sut = CreateSut(tenant, enforcer, out var exportStore);

        var command = ExportTenantDataCommand.FromDto(tenant.TenantId.Value, TestUserId.Value, TestCorrelationId.Value, TestCausationId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ContentReference.Should().Be("blob://tenant-exports/fake-reference.json");
        await exportStore.Received(1).SaveAsync(tenant.TenantId.Value, Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantWithNoConfiguredResidencyPolicy_DefaultsToUnrestricted_ExportsSuccessfully()
    {
        var tenant = CreateProvisionedTenant();
        var enforcer = CreateEnforcer(currentRegion: Region.Eu);
        var sut = CreateSut(tenant, enforcer, out var exportStore);

        var command = ExportTenantDataCommand.FromDto(tenant.TenantId.Value, TestUserId.Value, TestCorrelationId.Value, TestCausationId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await exportStore.Received(1).SaveAsync(tenant.TenantId.Value, Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }
}
