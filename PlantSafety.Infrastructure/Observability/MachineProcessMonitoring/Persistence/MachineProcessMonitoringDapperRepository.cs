// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Infrastructure
//  File:           MachineProcessMonitoringDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IMachineProcessMonitoringRepository (Domain repository
//                  interface for the MachineProcessMonitoring aggregate root). All DB access via stored
//                  procedures in the [Observability] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because MachineProcessMonitoring exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Enums;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Events;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Interfaces;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Specifications;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Observability.MachineProcessMonitoring.Persistence;

/// <summary>Dapper-backed repository for the <see cref="MachineProcessMonitoring"/> aggregate root.</summary>
public sealed class MachineProcessMonitoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<MachineProcessMonitoringDapperRepository> logger)
    : IMachineProcessMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring?> GetByIdAsync(MachineProcessMonitoringId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for MachineProcessMonitoring {MachineProcessMonitoringId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("MachineProcessMonitoringId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<MachineProcessMonitoringRow>(
            "[Observability].[usp_MachineProcessMonitoring_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring>> ListAsync(MachineProcessMonitoringSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List MachineProcessMonitoring via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<MachineProcessMonitoringRow>(
            "[Observability].[usp_MachineProcessMonitoring_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add MachineProcessMonitoring {MachineProcessMonitoringId}", aggregate.MachineProcessMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MachineProcessMonitoringId", aggregate.MachineProcessMonitoringId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("MachineId", aggregate.MachineId)
            .AddString("SensorType", aggregate.SensorType, 400)
            .AddDecimal("Reading", aggregate.Reading)
            .AddBool("ThresholdBreached", aggregate.ThresholdBreached)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddString("AnomalyDescription", aggregate.AnomalyDescription, 400)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_MachineProcessMonitoring_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    
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

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update MachineProcessMonitoring {MachineProcessMonitoringId}", aggregate.MachineProcessMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MachineProcessMonitoringId", aggregate.MachineProcessMonitoringId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("MachineId", aggregate.MachineId)
            .AddString("SensorType", aggregate.SensorType, 400)
            .AddDecimal("Reading", aggregate.Reading)
            .AddBool("ThresholdBreached", aggregate.ThresholdBreached)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddString("AnomalyDescription", aggregate.AnomalyDescription, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_MachineProcessMonitoring_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="MachineProcessMonitoring"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring Hydrate(MachineProcessMonitoringRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring>();

        AggregateReflectionHydrator.SetProperty(agg, "MachineProcessMonitoringId", MachineProcessMonitoringId.FromGuid(row.MachineProcessMonitoringId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<MachineProcessMonitoringStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "MachineId", row.MachineId);
        AggregateReflectionHydrator.SetProperty(agg, "SensorType", row.SensorType);
        AggregateReflectionHydrator.SetProperty(agg, "Reading", row.Reading);
        AggregateReflectionHydrator.SetProperty(agg, "ThresholdBreached", row.ThresholdBreached);
        AggregateReflectionHydrator.SetProperty(agg, "AcknowledgedBy", row.AcknowledgedBy.HasValue ? new UserId(row.AcknowledgedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "AnomalyDescription", row.AnomalyDescription);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IMachineProcessMonitoringDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_MachineProcessMonitoring_GetById</c>/<c>usp_MachineProcessMonitoring_GetAllActive</c>.</summary>
    private sealed record MachineProcessMonitoringRow
    {
        public required Guid MachineProcessMonitoringId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid MachineId { get; init; }
        public required string SensorType { get; init; }
        public required decimal Reading { get; init; }
        public required bool ThresholdBreached { get; init; }
        public Guid? AcknowledgedBy { get; init; }
        public string? AnomalyDescription { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
