// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Infrastructure
//  File:           PresenceSensingDevicesDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PresenceSensingDevices aggregate. Implements both the
//                  Domain-layer write repository (IPresenceSensingDevicesRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetPresenceSensingDevicesItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  PresenceSensingDevices exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Specifications;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.PresenceSensingDevices.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices;
using IWriteRepo = PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces.IPresenceSensingDevicesRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.PresenceSensingDevices.Repositories.IPresenceSensingDevicesRepository;

namespace PlantSafety.Infrastructure.MachineSafety.PresenceSensingDevices.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[PresenceSensingDevices], column-for-column with the PresenceSensingDevices aggregate.</summary>
internal sealed class PresenceSensingDevicesRow
{
    public Guid PresenceSensingDevicesId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? DeviceTitle { get; init; }
    public string? DeviceType { get; init; }
    public string? EquipmentLocation { get; init; }
    public Guid CalibratedByUserId { get; init; }
    public DateTime? CalibratedAtUtc { get; init; }
    public DateTime? NextCalibrationDueAtUtc { get; init; }
    public int TriggerEventCount { get; init; }
    public decimal? DetectionAccuracyScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the PresenceSensingDevices aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class PresenceSensingDevicesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PresenceSensingDevicesDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(PresenceSensingDevicesId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PresenceSensingDevices {PresenceSensingDevicesId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PresenceSensingDevicesId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PresenceSensingDevicesRow>(
            $"[{Schema}].[usp_PresenceSensingDevices_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, PresenceSensingDevicesRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(PresenceSensingDevicesSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for PresenceSensingDevices");

        var rows = await queryExecutor.QueryAsync<PresenceSensingDevicesRow>(
            $"[{Schema}].[usp_PresenceSensingDevices_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, PresenceSensingDevicesRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for PresenceSensingDevices {PresenceSensingDevicesId}", aggregate.PresenceSensingDevicesId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("PresenceSensingDevicesId", aggregate, "PresenceSensingDevicesId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DeviceTitle", aggregate, "DeviceTitle")
            .AddAggregateProperty("DeviceType", aggregate, "DeviceType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("CalibratedByUserId", aggregate, "CalibratedByUserId")
            .AddAggregateProperty("CalibratedAtUtc", aggregate, "CalibratedAtUtc")
            .AddAggregateProperty("NextCalibrationDueAtUtc", aggregate, "NextCalibrationDueAtUtc")
            .AddAggregateProperty("TriggerEventCount", aggregate, "TriggerEventCount")
            .AddAggregateProperty("DetectionAccuracyScore", aggregate, "DetectionAccuracyScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PresenceSensingDevices_Create]", parameters, cancellationToken: cancellationToken);
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for PresenceSensingDevices {PresenceSensingDevicesId}", aggregate.PresenceSensingDevicesId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("PresenceSensingDevicesId", aggregate, "PresenceSensingDevicesId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DeviceTitle", aggregate, "DeviceTitle")
            .AddAggregateProperty("DeviceType", aggregate, "DeviceType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("CalibratedByUserId", aggregate, "CalibratedByUserId")
            .AddAggregateProperty("CalibratedAtUtc", aggregate, "CalibratedAtUtc")
            .AddAggregateProperty("NextCalibrationDueAtUtc", aggregate, "NextCalibrationDueAtUtc")
            .AddAggregateProperty("TriggerEventCount", aggregate, "TriggerEventCount")
            .AddAggregateProperty("DetectionAccuracyScore", aggregate, "DetectionAccuracyScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PresenceSensingDevices_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetPresenceSensingDevicesItemsAsync"/>
    public async Task<IReadOnlyList<PresenceSensingDevicesItemDto>> GetPresenceSensingDevicesItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPresenceSensingDevicesItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PresenceSensingDevicesItemDto>(
            $"[{Schema}].[usp_PresenceSensingDevices_GetItems]", parameters, cancellationToken);
    }
}
