// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           SafetyZoneRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the SafetyZone aggregate --
//                  exercises the real, DI-resolved ISafetyZoneRepository (SafetyZoneDapperRepository)
//                  against the real [PhysicalAccessControl].[SafetyZones] table and
//                  usp_SafetyZones_Create/Update/GetById/GetOccupancyItem stored procedures.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Enums;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Repositories;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using SafetyZoneAggregate = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class SafetyZoneRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripThroughRealStoredProcedures()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISafetyZoneRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var aggregate = SafetyZoneAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId,
            zoneCode: $"ZN-IT-{Guid.NewGuid():N}"[..10],
            zoneName: "Integration Test Zone",
            hazardLevel: SafetyZoneHazardLevel.Critical,
            maxOccupancy: 8);

        await repository.AddAsync(aggregate, CancellationToken.None);

        var fetched = await repository.GetByIdAsync(aggregate.SafetyZoneId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.HazardLevel.Should().Be(SafetyZoneHazardLevel.Critical);
        fetched.MaxOccupancy.Should().Be(8);
        fetched.CurrentOccupancy.Should().Be(0);
        fetched.Status.Should().Be(SafetyZoneStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_AfterOccupancyRecalculation_ShouldPersistNewOccupancy()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISafetyZoneRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = SafetyZoneAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, zoneCode: $"ZN-IT-{Guid.NewGuid():N}"[..10], zoneName: "Occupancy Test Zone", maxOccupancy: 20);

        await repository.AddAsync(aggregate, CancellationToken.None);

        aggregate.UpdateOccupancy(14, UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(aggregate, CancellationToken.None);

        var afterUpdate = await repository.GetByIdAsync(aggregate.SafetyZoneId, CancellationToken.None);

        afterUpdate!.CurrentOccupancy.Should().Be(14);
    }

    [Fact]
    public async Task ReadModel_GetOccupancyAsync_ShouldReturnCurrentOccupancyProjection()
    {
        using var scope = Factory.Services.CreateScope();
        var writeRepository = scope.ServiceProvider.GetRequiredService<ISafetyZoneRepository>();
        var readRepository = scope.ServiceProvider.GetRequiredService<ISafetyZoneReadRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = SafetyZoneAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, zoneCode: $"ZN-IT-{Guid.NewGuid():N}"[..10], zoneName: "Read-Model Test Zone", maxOccupancy: 30);

        await writeRepository.AddAsync(aggregate, CancellationToken.None);

        aggregate.UpdateOccupancy(9, UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await writeRepository.UpdateAsync(aggregate, CancellationToken.None);

        var occupancy = await readRepository.GetOccupancyAsync(TenantId, aggregate.SafetyZoneId.Value, CancellationToken.None);

        occupancy.Should().NotBeNull();
        occupancy!.CurrentOccupancy.Should().Be(9);
        occupancy.MaxOccupancy.Should().Be(30);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISafetyZoneRepository>();

        var fetched = await repository.GetByIdAsync(
            PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids.SafetyZoneId.New(), CancellationToken.None);

        fetched.Should().BeNull();
    }
}
