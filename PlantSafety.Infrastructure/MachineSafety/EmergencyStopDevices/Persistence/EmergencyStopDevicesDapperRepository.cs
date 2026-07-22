// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Infrastructure
//  File:           EmergencyStopDevicesDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencyStopDevices aggregate. Implements both the
//                  Domain-layer write repository (IEmergencyStopDevicesRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetEmergencyStopDevicesItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  EmergencyStopDevices exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Specifications;
using PlantSafety.Domain.MachineSafety.EmergencyStopDevices.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.EmergencyStopDevices.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices;
using IWriteRepo = PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Interfaces.IEmergencyStopDevicesRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.EmergencyStopDevices.Repositories.IEmergencyStopDevicesRepository;

namespace PlantSafety.Infrastructure.MachineSafety.EmergencyStopDevices.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[EmergencyStopDevices], column-for-column with the EmergencyStopDevices aggregate.</summary>
internal sealed class EmergencyStopDevicesRow
{
    public Guid EmergencyStopDevicesId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? DeviceTitle { get; init; }
    public string? DeviceType { get; init; }
    public string? EquipmentLocation { get; init; }
    public Guid TestedByUserId { get; init; }
    public DateTime? TestedAtUtc { get; init; }
    public DateTime? NextTestDueAtUtc { get; init; }
    public int FailureCount { get; init; }
    public decimal? ResponseTimeScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EmergencyStopDevices aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class EmergencyStopDevicesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EmergencyStopDevicesDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EmergencyStopDevicesId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EmergencyStopDevices {EmergencyStopDevicesId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyStopDevicesId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EmergencyStopDevicesRow>(
            $"[{Schema}].[usp_EmergencyStopDevices_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EmergencyStopDevicesRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EmergencyStopDevicesSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for EmergencyStopDevices");

        var rows = await queryExecutor.QueryAsync<EmergencyStopDevicesRow>(
            $"[{Schema}].[usp_EmergencyStopDevices_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, EmergencyStopDevicesRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for EmergencyStopDevices {EmergencyStopDevicesId}", aggregate.EmergencyStopDevicesId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EmergencyStopDevicesId", aggregate, "EmergencyStopDevicesId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DeviceTitle", aggregate, "DeviceTitle")
            .AddAggregateProperty("DeviceType", aggregate, "DeviceType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("TestedByUserId", aggregate, "TestedByUserId")
            .AddAggregateProperty("TestedAtUtc", aggregate, "TestedAtUtc")
            .AddAggregateProperty("NextTestDueAtUtc", aggregate, "NextTestDueAtUtc")
            .AddAggregateProperty("FailureCount", aggregate, "FailureCount")
            .AddAggregateProperty("ResponseTimeScore", aggregate, "ResponseTimeScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EmergencyStopDevices_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for EmergencyStopDevices {EmergencyStopDevicesId}", aggregate.EmergencyStopDevicesId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EmergencyStopDevicesId", aggregate, "EmergencyStopDevicesId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DeviceTitle", aggregate, "DeviceTitle")
            .AddAggregateProperty("DeviceType", aggregate, "DeviceType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("TestedByUserId", aggregate, "TestedByUserId")
            .AddAggregateProperty("TestedAtUtc", aggregate, "TestedAtUtc")
            .AddAggregateProperty("NextTestDueAtUtc", aggregate, "NextTestDueAtUtc")
            .AddAggregateProperty("FailureCount", aggregate, "FailureCount")
            .AddAggregateProperty("ResponseTimeScore", aggregate, "ResponseTimeScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EmergencyStopDevices_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetEmergencyStopDevicesItemsAsync"/>
    public async Task<IReadOnlyList<EmergencyStopDevicesItemDto>> GetEmergencyStopDevicesItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencyStopDevicesItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencyStopDevicesItemDto>(
            $"[{Schema}].[usp_EmergencyStopDevices_GetItems]", parameters, cancellationToken);
    }
}
