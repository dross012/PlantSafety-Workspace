// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           AccessAnomalyRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the AccessAnomaly aggregate --
//                  exercises the real, DI-resolved IAccessAnomalyRepository
//                  (AccessAnomalyDapperRepository) against the real
//                  [PhysicalAccessControl].[AccessAnomalies] table and
//                  usp_AccessAnomalies_Create/Update/GetById/ListActiveItems stored procedures.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Repositories;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using AccessAnomalyAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AccessAnomalyRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripThroughRealStoredProcedures()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessAnomalyRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var aggregate = AccessAnomalyAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId,
            accessPointId: Guid.NewGuid(),
            anomalyType: AccessAnomalyType.Tailgating,
            severityScore: 72.5m);

        await repository.AddAsync(aggregate, CancellationToken.None);

        var fetched = await repository.GetByIdAsync(aggregate.AccessAnomalyId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.AnomalyType.Should().Be(AccessAnomalyType.Tailgating);
        fetched.SeverityScore.Should().Be(72.5m);
        fetched.Status.Should().Be(AccessAnomalyStatus.Detected);
    }

    [Fact]
    public async Task UpdateAsync_AfterAcknowledgeAndResolve_ShouldPersistFullTriageLifecycle()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessAnomalyRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = AccessAnomalyAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid(), severityScore: 40m);

        await repository.AddAsync(aggregate, CancellationToken.None);

        var ackByUserId = Guid.NewGuid();
        aggregate.Acknowledge(ackByUserId, UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(aggregate, CancellationToken.None);

        var afterAck = await repository.GetByIdAsync(aggregate.AccessAnomalyId, CancellationToken.None);
        afterAck!.Status.Should().Be(AccessAnomalyStatus.Acknowledged);
        afterAck.AcknowledgedByUserId.Should().Be(ackByUserId);

        afterAck.Resolve("Confirmed false positive.", UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(afterAck, CancellationToken.None);

        var afterResolve = await repository.GetByIdAsync(aggregate.AccessAnomalyId, CancellationToken.None);
        afterResolve!.Status.Should().Be(AccessAnomalyStatus.Resolved);
        afterResolve.Resolution.Should().Be("Confirmed false positive.");
    }

    [Fact]
    public async Task ReadModel_ListActiveAsync_ShouldExcludeResolvedAnomalies()
    {
        using var scope = Factory.Services.CreateScope();
        var writeRepository = scope.ServiceProvider.GetRequiredService<IAccessAnomalyRepository>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IAccessAnomalyReadRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);

        var active = AccessAnomalyAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid(), severityScore: 60m);
        var resolved = AccessAnomalyAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointId: Guid.NewGuid(), severityScore: 15m);

        await writeRepository.AddAsync(active, CancellationToken.None);
        await writeRepository.AddAsync(resolved, CancellationToken.None);

        resolved.Acknowledge(Guid.NewGuid(), UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        resolved.Resolve("Handled", UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await writeRepository.UpdateAsync(resolved, CancellationToken.None);

        var items = await readRepository.ListActiveAsync(TenantId, null, CancellationToken.None);

        items.Should().Contain(x => x.AccessAnomalyId == active.AccessAnomalyId.Value);
        items.Should().NotContain(x => x.AccessAnomalyId == resolved.AccessAnomalyId.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessAnomalyRepository>();

        var fetched = await repository.GetByIdAsync(
            PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids.AccessAnomalyId.New(), CancellationToken.None);

        fetched.Should().BeNull();
    }
}
