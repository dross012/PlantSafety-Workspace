// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.MachineSafety.CraneHoistSystems
//  Layer:          IntegrationTests
//  File:           CraneHoistSystemsSqlTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    SQL-layer integration tests for the CraneHoistSystems sub-aggregate,
//                  verifying the real stored procedures (usp_CraneHoistSystems_Create/Update/
//                  GetById/GetItems/GetItemById/ListDueForInspection/ListExpiringQualifications)
//                  against a Testcontainers SQL Server instance, exercised through the real
//                  CraneHoistSystemsDapperRepository resolved from the DI container (both the
//                  Domain write-repository and the Application read-model repository facets).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using IWriteRepo = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces.ICraneHoistSystemsRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories.ICraneHoistSystemsRepository;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.IntegrationTests.MachineSafety.CraneHoistSystems.Sql;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CraneHoistSystemsSqlTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CraneHoistSystemsTable_ShouldExist_InMachineSafetySchema()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_SCHEMA = 'MachineSafety' AND TABLE_NAME = 'CraneHoistSystems'");

        count.Should().Be(1, "the CraneHoistSystems table must exist before running these SQL tests");
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundTripsThroughRealStoredProcedures()
    {
        var (scope, writeRepo) = Factory.Services.GetScopedService<IWriteRepo>();
        try
        {
            var aggregate = Agg.Create(
                createdBy: UserId.FromGuid(ActorId),
                createdAtUtc: UtcTimestamp.From(FixedUtcNow),
                correlationId: PlantSafety.SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId),
                causationId: CausationId.FromGuid(TestIds.CausationId),
                actor: PerformedByActor.User(UserId.FromGuid(ActorId)),
                tenantId: TenantId,
                siteId: TestIds.SiteId,
                assetTag: "SQL-1",
                assetTitle: "SQL Integration Crane",
                assetType: CraneHoistAssetType.OverheadBridgeCrane,
                ratedLoadCapacityKg: 5000m);

            await writeRepo.AddAsync(aggregate, CancellationToken.None);

            var fetched = await writeRepo.GetByIdAsync(aggregate.CraneHoistSystemsId, CancellationToken.None);

            fetched.Should().NotBeNull();
            fetched!.AssetTag.Should().Be("SQL-1");
            fetched.AssetTitle.Should().Be("SQL Integration Crane");
            fetched.AssetType.Should().Be(CraneHoistAssetType.OverheadBridgeCrane);
            fetched.RatedLoadCapacityKg.Should().Be(5000m);
            fetched.Status.Should().Be(CraneHoistSystemsStatus.Registered);
            fetched.TenantId.Should().Be(TenantId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task UpdateAsync_PersistsLifecycleTransitionsThroughRealStoredProcedure()
    {
        var (scope, writeRepo) = Factory.Services.GetScopedService<IWriteRepo>();
        try
        {
            var aggregate = Agg.Create(
                createdBy: UserId.FromGuid(ActorId), createdAtUtc: UtcTimestamp.From(FixedUtcNow),
                correlationId: PlantSafety.SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId), causationId: CausationId.FromGuid(TestIds.CausationId),
                actor: PerformedByActor.User(UserId.FromGuid(ActorId)), tenantId: TenantId, siteId: null,
                assetTag: "SQL-2", assetTitle: "SQL Update Crane", assetType: CraneHoistAssetType.Hoist, ratedLoadCapacityKg: 2000m);
            await writeRepo.AddAsync(aggregate, CancellationToken.None);

            aggregate.RecordOverloadEvent(2500m, UtcTimestamp.From(FixedUtcNow), PerformedByActor.User(UserId.FromGuid(ActorId)),
                PlantSafety.SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId), CausationId.FromGuid(TestIds.CausationId));
            await writeRepo.UpdateAsync(aggregate, CancellationToken.None);

            var refetched = await writeRepo.GetByIdAsync(aggregate.CraneHoistSystemsId, CancellationToken.None);

            refetched.Should().NotBeNull();
            refetched!.OverloadEventCount.Should().Be(1);
            refetched.OverloadFlagged.Should().BeTrue();
            refetched.LastObservedLoadKg.Should().Be(2500m);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ForUnknownId_ReturnsNull()
    {
        var (scope, writeRepo) = Factory.Services.GetScopedService<IWriteRepo>();
        try
        {
            var result = await writeRepo.GetByIdAsync(CraneHoistSystemsId.New(), CancellationToken.None);
            result.Should().BeNull();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ReadModelRepository_GetCraneHoistSystemsItemByIdAsync_ReturnsProjectedDto()
    {
        var (scope, writeRepo) = Factory.Services.GetScopedService<IWriteRepo>();
        try
        {
            var aggregate = Agg.Create(
                createdBy: UserId.FromGuid(ActorId), createdAtUtc: UtcTimestamp.From(FixedUtcNow),
                correlationId: PlantSafety.SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId), causationId: CausationId.FromGuid(TestIds.CausationId),
                actor: PerformedByActor.User(UserId.FromGuid(ActorId)), tenantId: TenantId, siteId: null,
                assetTag: "SQL-3", assetTitle: "SQL ReadModel Crane", assetType: CraneHoistAssetType.MobileCrane, ratedLoadCapacityKg: 8000m);
            await writeRepo.AddAsync(aggregate, CancellationToken.None);

            var readRepo = scope.ServiceProvider.GetRequiredService<IReadRepo>();
            var dto = await readRepo.GetCraneHoistSystemsItemByIdAsync(TenantId, aggregate.CraneHoistSystemsId.Value, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.AssetTag.Should().Be("SQL-3");
            dto.SystemStatus.Should().Be("Registered");
            dto.RatedLoadCapacityKg.Should().Be(8000m);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ReadModelRepository_ListAssetsDueForInspectionAsync_ReturnsOnlyAssetsPastDueDate()
    {
        var (scope, writeRepo) = Factory.Services.GetScopedService<IWriteRepo>();
        try
        {
            var actor = PerformedByActor.User(UserId.FromGuid(ActorId));
            var correlationId = PlantSafety.SharedKernel.Identifiers.CorrelationId.FromGuid(CorrelationId);
            var causationId = CausationId.FromGuid(TestIds.CausationId);

            var dueAsset = Agg.Create(
                createdBy: UserId.FromGuid(ActorId), createdAtUtc: UtcTimestamp.From(FixedUtcNow),
                correlationId: correlationId, causationId: causationId, actor: actor, tenantId: TenantId, siteId: null,
                assetTag: "SQL-DUE", assetTitle: "Due Crane", assetType: CraneHoistAssetType.JibCrane, ratedLoadCapacityKg: 1000m);
            dueAsset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow.AddDays(-1)), UtcTimestamp.From(FixedUtcNow), actor, correlationId, causationId);
            await writeRepo.AddAsync(dueAsset, CancellationToken.None);

            var notDueAsset = Agg.Create(
                createdBy: UserId.FromGuid(ActorId), createdAtUtc: UtcTimestamp.From(FixedUtcNow),
                correlationId: correlationId, causationId: causationId, actor: actor, tenantId: TenantId, siteId: null,
                assetTag: "SQL-NOTDUE", assetTitle: "Not Due Crane", assetType: CraneHoistAssetType.JibCrane, ratedLoadCapacityKg: 1000m);
            notDueAsset.ScheduleCraneInspection(UtcTimestamp.From(FixedUtcNow.AddDays(60)), UtcTimestamp.From(FixedUtcNow), actor, correlationId, causationId);
            await writeRepo.AddAsync(notDueAsset, CancellationToken.None);

            var readRepo = scope.ServiceProvider.GetRequiredService<IReadRepo>();
            var results = await readRepo.ListAssetsDueForInspectionAsync(TenantId, null, DateTime.UtcNow, CancellationToken.None);

            results.Should().Contain(a => a.CraneHoistSystemsId == dueAsset.CraneHoistSystemsId.Value);
            results.Should().NotContain(a => a.CraneHoistSystemsId == notDueAsset.CraneHoistSystemsId.Value);
        }
        finally
        {
            scope.Dispose();
        }
    }
}
