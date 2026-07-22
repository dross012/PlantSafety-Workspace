// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Infrastructure
//  File:           SafetyValvesRelaysDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyValvesRelays aggregate. Implements both the
//                  Domain-layer write repository (ISafetyValvesRelaysRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetSafetyValvesRelaysItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  SafetyValvesRelays exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Specifications;
using PlantSafety.Domain.MachineSafety.SafetyValvesRelays.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.SafetyValvesRelays.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays;
using IWriteRepo = PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Interfaces.ISafetyValvesRelaysRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.SafetyValvesRelays.Repositories.ISafetyValvesRelaysRepository;

namespace PlantSafety.Infrastructure.MachineSafety.SafetyValvesRelays.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[SafetyValvesRelays], column-for-column with the SafetyValvesRelays aggregate.</summary>
internal sealed class SafetyValvesRelaysRow
{
    public Guid SafetyValvesRelaysId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ComponentTitle { get; init; }
    public string? ComponentType { get; init; }
    public string? EquipmentLocation { get; init; }
    public Guid InspectedByUserId { get; init; }
    public DateTime? InspectedAtUtc { get; init; }
    public DateTime? NextInspectionDueAtUtc { get; init; }
    public int FaultCount { get; init; }
    public decimal? ReliabilityScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the SafetyValvesRelays aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class SafetyValvesRelaysDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SafetyValvesRelaysDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(SafetyValvesRelaysId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for SafetyValvesRelays {SafetyValvesRelaysId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SafetyValvesRelaysId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SafetyValvesRelaysRow>(
            $"[{Schema}].[usp_SafetyValvesRelays_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, SafetyValvesRelaysRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(SafetyValvesRelaysSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for SafetyValvesRelays");

        var rows = await queryExecutor.QueryAsync<SafetyValvesRelaysRow>(
            $"[{Schema}].[usp_SafetyValvesRelays_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, SafetyValvesRelaysRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for SafetyValvesRelays {SafetyValvesRelaysId}", aggregate.SafetyValvesRelaysId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("SafetyValvesRelaysId", aggregate, "SafetyValvesRelaysId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ComponentTitle", aggregate, "ComponentTitle")
            .AddAggregateProperty("ComponentType", aggregate, "ComponentType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("InspectedByUserId", aggregate, "InspectedByUserId")
            .AddAggregateProperty("InspectedAtUtc", aggregate, "InspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("FaultCount", aggregate, "FaultCount")
            .AddAggregateProperty("ReliabilityScore", aggregate, "ReliabilityScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SafetyValvesRelays_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for SafetyValvesRelays {SafetyValvesRelaysId}", aggregate.SafetyValvesRelaysId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("SafetyValvesRelaysId", aggregate, "SafetyValvesRelaysId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ComponentTitle", aggregate, "ComponentTitle")
            .AddAggregateProperty("ComponentType", aggregate, "ComponentType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("InspectedByUserId", aggregate, "InspectedByUserId")
            .AddAggregateProperty("InspectedAtUtc", aggregate, "InspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("FaultCount", aggregate, "FaultCount")
            .AddAggregateProperty("ReliabilityScore", aggregate, "ReliabilityScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SafetyValvesRelays_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetSafetyValvesRelaysItemsAsync"/>
    public async Task<IReadOnlyList<SafetyValvesRelaysItemDto>> GetSafetyValvesRelaysItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSafetyValvesRelaysItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SafetyValvesRelaysItemDto>(
            $"[{Schema}].[usp_SafetyValvesRelays_GetItems]", parameters, cancellationToken);
    }
}
