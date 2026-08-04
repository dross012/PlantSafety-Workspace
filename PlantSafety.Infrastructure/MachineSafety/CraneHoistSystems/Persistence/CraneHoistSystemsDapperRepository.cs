// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Infrastructure
//  File:           CraneHoistSystemsDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CraneHoistSystems aggregate. Implements both the
//                  Domain-layer write repository (ICraneHoistSystemsRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name. All DB access via stored procedures;
//                  no dynamic SQL. Aggregate rehydration uses AggregateHydrator (reflection,
//                  confined to Infrastructure) because CraneHoistSystems exposes only a private
//                  parameterless constructor and private-set properties with no public
//                  Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Specifications;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;
using IWriteRepo = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces.ICraneHoistSystemsRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories.ICraneHoistSystemsRepository;

namespace PlantSafety.Infrastructure.MachineSafety.CraneHoistSystems.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[CraneHoistSystems], column-for-column with the CraneHoistSystems aggregate.</summary>
internal sealed class CraneHoistSystemsRow
{
    public Guid CraneHoistSystemsId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? AssetTag { get; init; }
    public string? AssetTitle { get; init; }
    public string? AssetType { get; init; }
    public decimal RatedLoadCapacityKg { get; init; }
    public decimal? LastObservedLoadKg { get; init; }
    public DateTime? LastLiftAtUtc { get; init; }
    public int OverloadEventCount { get; init; }
    public bool OverloadFlagged { get; init; }
    public DateTime? InspectionDueAtUtc { get; init; }
    public DateTime? LastInspectionAtUtc { get; init; }
    public bool? LastInspectionPassed { get; init; }
    public Guid OperatorUserId { get; init; }
    public DateTime? OperatorQualifiedAtUtc { get; init; }
    public DateTime? OperatorQualificationExpiresAtUtc { get; init; }
    public bool OperatorQualificationSuspended { get; init; }
    public DateTime? LastRiggingInspectionAtUtc { get; init; }
    public bool? LastRiggingDefectDetected { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the CraneHoistSystems aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CraneHoistSystemsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CraneHoistSystemsDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CraneHoistSystemsId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CraneHoistSystems {CraneHoistSystemsId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CraneHoistSystemsId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CraneHoistSystemsRow>(
            $"[{Schema}].[usp_CraneHoistSystems_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CraneHoistSystemsRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(CraneHoistSystemsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for CraneHoistSystems");

        var rows = await queryExecutor.QueryAsync<CraneHoistSystemsRow>(
            $"[{Schema}].[usp_CraneHoistSystems_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, CraneHoistSystemsRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for CraneHoistSystems {CraneHoistSystemsId}", aggregate.CraneHoistSystemsId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CraneHoistSystemsId", aggregate, "CraneHoistSystemsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AssetTag", aggregate, "AssetTag")
            .AddAggregateProperty("AssetTitle", aggregate, "AssetTitle")
            .AddAggregateProperty("AssetType", aggregate, "AssetType")
            .AddAggregateProperty("RatedLoadCapacityKg", aggregate, "RatedLoadCapacityKg")
            .AddAggregateProperty("LastObservedLoadKg", aggregate, "LastObservedLoadKg")
            .AddAggregateProperty("LastLiftAtUtc", aggregate, "LastLiftAtUtc")
            .AddAggregateProperty("OverloadEventCount", aggregate, "OverloadEventCount")
            .AddAggregateProperty("OverloadFlagged", aggregate, "OverloadFlagged")
            .AddAggregateProperty("InspectionDueAtUtc", aggregate, "InspectionDueAtUtc")
            .AddAggregateProperty("LastInspectionAtUtc", aggregate, "LastInspectionAtUtc")
            .AddAggregateProperty("LastInspectionPassed", aggregate, "LastInspectionPassed")
            .AddAggregateProperty("OperatorUserId", aggregate, "OperatorUserId")
            .AddAggregateProperty("OperatorQualifiedAtUtc", aggregate, "OperatorQualifiedAtUtc")
            .AddAggregateProperty("OperatorQualificationExpiresAtUtc", aggregate, "OperatorQualificationExpiresAtUtc")
            .AddAggregateProperty("OperatorQualificationSuspended", aggregate, "OperatorQualificationSuspended")
            .AddAggregateProperty("LastRiggingInspectionAtUtc", aggregate, "LastRiggingInspectionAtUtc")
            .AddAggregateProperty("LastRiggingDefectDetected", aggregate, "LastRiggingDefectDetected")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CraneHoistSystems_Create]", parameters, cancellationToken: cancellationToken);

        // Outbox wiring: enqueue any domain events raised by the aggregate during this write so
        // TransactionalUnitOfWork.CommitAsync (invoked by TransactionBehavior after the handler
        // returns) can flush them to [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for CraneHoistSystems {CraneHoistSystemsId}", aggregate.CraneHoistSystemsId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CraneHoistSystemsId", aggregate, "CraneHoistSystemsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AssetTag", aggregate, "AssetTag")
            .AddAggregateProperty("AssetTitle", aggregate, "AssetTitle")
            .AddAggregateProperty("AssetType", aggregate, "AssetType")
            .AddAggregateProperty("RatedLoadCapacityKg", aggregate, "RatedLoadCapacityKg")
            .AddAggregateProperty("LastObservedLoadKg", aggregate, "LastObservedLoadKg")
            .AddAggregateProperty("LastLiftAtUtc", aggregate, "LastLiftAtUtc")
            .AddAggregateProperty("OverloadEventCount", aggregate, "OverloadEventCount")
            .AddAggregateProperty("OverloadFlagged", aggregate, "OverloadFlagged")
            .AddAggregateProperty("InspectionDueAtUtc", aggregate, "InspectionDueAtUtc")
            .AddAggregateProperty("LastInspectionAtUtc", aggregate, "LastInspectionAtUtc")
            .AddAggregateProperty("LastInspectionPassed", aggregate, "LastInspectionPassed")
            .AddAggregateProperty("OperatorUserId", aggregate, "OperatorUserId")
            .AddAggregateProperty("OperatorQualifiedAtUtc", aggregate, "OperatorQualifiedAtUtc")
            .AddAggregateProperty("OperatorQualificationExpiresAtUtc", aggregate, "OperatorQualificationExpiresAtUtc")
            .AddAggregateProperty("OperatorQualificationSuspended", aggregate, "OperatorQualificationSuspended")
            .AddAggregateProperty("LastRiggingInspectionAtUtc", aggregate, "LastRiggingInspectionAtUtc")
            .AddAggregateProperty("LastRiggingDefectDetected", aggregate, "LastRiggingDefectDetected")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CraneHoistSystems_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetCraneHoistSystemsItemsAsync"/>
    public async Task<IReadOnlyList<CraneHoistSystemsItemDto>> GetCraneHoistSystemsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCraneHoistSystemsItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CraneHoistSystemsItemDto>(
            $"[{Schema}].[usp_CraneHoistSystems_GetItems]", parameters, cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetCraneHoistSystemsItemByIdAsync"/>
    public async Task<CraneHoistSystemsItemDto?> GetCraneHoistSystemsItemByIdAsync(
        Guid tenantId,
        Guid craneHoistSystemsId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCraneHoistSystemsItemByIdAsync for tenant {TenantId}, asset {CraneHoistSystemsId}", tenantId, craneHoistSystemsId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CraneHoistSystemsId", craneHoistSystemsId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CraneHoistSystemsItemDto>(
            $"[{Schema}].[usp_CraneHoistSystems_GetItemById]", parameters, cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.ListAssetsDueForInspectionAsync"/>
    public async Task<IReadOnlyList<CraneHoistSystemsItemDto>> ListAssetsDueForInspectionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime asOfUtc,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListAssetsDueForInspectionAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("AsOfUtc", asOfUtc)
            .Build();

        return await queryExecutor.QueryAsync<CraneHoistSystemsItemDto>(
            $"[{Schema}].[usp_CraneHoistSystems_ListDueForInspection]", parameters, cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.ListExpiringOperatorQualificationsAsync"/>
    public async Task<IReadOnlyList<CraneHoistSystemsItemDto>> ListExpiringOperatorQualificationsAsync(
        Guid tenantId,
        Guid? siteId,
        int withinDays,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListExpiringOperatorQualificationsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddInt("WithinDays", withinDays)
            .Build();

        return await queryExecutor.QueryAsync<CraneHoistSystemsItemDto>(
            $"[{Schema}].[usp_CraneHoistSystems_ListExpiringQualifications]", parameters, cancellationToken);
    }
}
