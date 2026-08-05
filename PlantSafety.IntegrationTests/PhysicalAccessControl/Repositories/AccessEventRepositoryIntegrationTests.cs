// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           AccessEventRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the AccessEvent aggregate --
//                  exercises the real, DI-resolved IAccessEventRepository (AccessEventDapperRepository)
//                  against the real [PhysicalAccessControl].[AccessEvents] table and
//                  usp_AccessEvents_Create/Update/GetById/ListByZoneItems stored procedures.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Repositories;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using AccessEventAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AccessEventRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripThroughRealStoredProcedures()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessEventRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var accessPointId = Guid.NewGuid();
        var occurredAt = UtcTimestamp.From(DateTime.UtcNow.AddMinutes(-2));
        var aggregate = AccessEventAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId,
            accessPointId: accessPointId,
            eventType: AccessEventType.Denied,
            direction: AccessEventDirection.Out,
            occurredAtUtc: occurredAt);

        await repository.AddAsync(aggregate, CancellationToken.None);

        var fetched = await repository.GetByIdAsync(aggregate.AccessEventId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.AccessPointId.Should().Be(accessPointId);
        fetched.EventType.Should().Be(AccessEventType.Denied);
        fetched.Direction.Should().Be(AccessEventDirection.Out);
        fetched.Status.Should().Be(AccessEventStatus.Ingested);
    }

    [Fact]
    public async Task UpdateAsync_AfterFlagAnomaly_ShouldPersistFlaggedAnomalyType()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessEventRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = AccessEventAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid());

        await repository.AddAsync(aggregate, CancellationToken.None);

        aggregate.FlagAnomaly("ForcedEntry", UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(aggregate, CancellationToken.None);

        var afterFlag = await repository.GetByIdAsync(aggregate.AccessEventId, CancellationToken.None);

        afterFlag!.Status.Should().Be(AccessEventStatus.Flagged);
        afterFlag.FlaggedAnomalyType.Should().Be("ForcedEntry");
    }

    [Fact]
    public async Task ReadModel_ListByZoneAsync_ShouldReturnOnlyEventsWithinTheRequestedZoneAndDateRange()
    {
        using var scope = Factory.Services.CreateScope();
        var writeRepository = scope.ServiceProvider.GetRequiredService<IAccessEventRepository>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IAccessEventReadRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var targetZone = Guid.NewGuid();
        var otherZone = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var inRange = AccessEventAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid(), safetyZoneId: targetZone,
            occurredAtUtc: UtcTimestamp.From(now.AddHours(-1)));
        var outOfZone = AccessEventAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid(), safetyZoneId: otherZone,
            occurredAtUtc: UtcTimestamp.From(now.AddHours(-1)));
        var outOfRange = AccessEventAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid(), safetyZoneId: targetZone,
            occurredAtUtc: UtcTimestamp.From(now.AddDays(-10)));

        await writeRepository.AddAsync(inRange, CancellationToken.None);
        await writeRepository.AddAsync(outOfZone, CancellationToken.None);
        await writeRepository.AddAsync(outOfRange, CancellationToken.None);

        var items = await readRepository.ListByZoneAsync(TenantId, targetZone, now.AddDays(-1), now.AddDays(1), CancellationToken.None);

        items.Should().Contain(x => x.AccessEventId == inRange.AccessEventId.Value);
        items.Should().NotContain(x => x.AccessEventId == outOfZone.AccessEventId.Value);
        items.Should().NotContain(x => x.AccessEventId == outOfRange.AccessEventId.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessEventRepository>();

        var fetched = await repository.GetByIdAsync(
            PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids.AccessEventId.New(), CancellationToken.None);

        fetched.Should().BeNull();
    }
}
