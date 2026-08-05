// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           RestrictedZonePolicyRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the RestrictedZonePolicy
//                  aggregate -- exercises the real, DI-resolved IRestrictedZonePolicyRepository
//                  (RestrictedZonePolicyDapperRepository) against the real
//                  [PhysicalAccessControl].[RestrictedZonePolicies] table and
//                  usp_RestrictedZonePolicies_Create/Update/GetById stored procedures.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Enums;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using RestrictedZonePolicyAggregate = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class RestrictedZonePolicyRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripThroughRealStoredProcedures()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRestrictedZonePolicyRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var zoneId = Guid.NewGuid();
        var aggregate = RestrictedZonePolicyAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId,
            safetyZoneId: zoneId,
            policyName: "Integration Test Restricted Policy",
            allowedAccessLevel: "HighVoltageClearance",
            enforceAfterHoursLockout: true);

        await repository.AddAsync(aggregate, CancellationToken.None);

        var fetched = await repository.GetByIdAsync(aggregate.RestrictedZonePolicyId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.SafetyZoneId.Should().Be(zoneId);
        fetched.AllowedAccessLevel.Should().Be("HighVoltageClearance");
        fetched.EnforceAfterHoursLockout.Should().BeTrue();
        fetched.Status.Should().Be(RestrictedZonePolicyStatus.Draft);
    }

    [Fact]
    public async Task UpdateAsync_AfterActivateThenSuspend_ShouldPersistLifecycleTransitions()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRestrictedZonePolicyRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = RestrictedZonePolicyAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, safetyZoneId: Guid.NewGuid(), policyName: "Lifecycle Test Policy");

        await repository.AddAsync(aggregate, CancellationToken.None);

        aggregate.Activate(UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(aggregate, CancellationToken.None);

        var afterActivate = await repository.GetByIdAsync(aggregate.RestrictedZonePolicyId, CancellationToken.None);
        afterActivate!.Status.Should().Be(RestrictedZonePolicyStatus.Active);

        afterActivate.Suspend(UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(afterActivate, CancellationToken.None);

        var afterSuspend = await repository.GetByIdAsync(aggregate.RestrictedZonePolicyId, CancellationToken.None);
        afterSuspend!.Status.Should().Be(RestrictedZonePolicyStatus.Suspended);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRestrictedZonePolicyRepository>();

        var fetched = await repository.GetByIdAsync(
            PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids.RestrictedZonePolicyId.New(), CancellationToken.None);

        fetched.Should().BeNull();
    }
}
