// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Infrastructure
//  File:           PPEBehavioralMonitoringDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPEBehavioralMonitoringRepository (Domain repository
//                  interface for the PPEBehavioralMonitoring aggregate root). All DB access via stored
//                  procedures in the [Observability] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PPEBehavioralMonitoring exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates;
using PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Enums;
using PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Events;
using PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Interfaces;
using PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Specifications;
using PlantSafety.Domain.Observability.PPEBehavioralMonitoring.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Observability.PPEBehavioralMonitoring.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PPEBehavioralMonitoring"/> aggregate root.</summary>
public sealed class PPEBehavioralMonitoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PPEBehavioralMonitoringDapperRepository> logger)
    : IPPEBehavioralMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring?> GetByIdAsync(PPEBehavioralMonitoringId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PPEBehavioralMonitoring {PPEBehavioralMonitoringId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PPEBehavioralMonitoringId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PPEBehavioralMonitoringRow>(
            "[Observability].[usp_PPEBehavioralMonitoring_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring>> ListAsync(PPEBehavioralMonitoringSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PPEBehavioralMonitoring via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PPEBehavioralMonitoringRow>(
            "[Observability].[usp_PPEBehavioralMonitoring_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PPEBehavioralMonitoring {PPEBehavioralMonitoringId}", aggregate.PPEBehavioralMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEBehavioralMonitoringId", aggregate.PPEBehavioralMonitoringId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SensorId", aggregate.SensorId, 400)
            .AddString("ZoneCode", aggregate.ZoneCode, 400)
            .AddString("DetectedEventType", aggregate.DetectedEventType, 400)
            .AddDecimal("ConfidenceScore", aggregate.ConfidenceScore)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_PPEBehavioralMonitoring_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PPEBehavioralMonitoring {PPEBehavioralMonitoringId}", aggregate.PPEBehavioralMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEBehavioralMonitoringId", aggregate.PPEBehavioralMonitoringId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SensorId", aggregate.SensorId, 400)
            .AddString("ZoneCode", aggregate.ZoneCode, 400)
            .AddString("DetectedEventType", aggregate.DetectedEventType, 400)
            .AddDecimal("ConfidenceScore", aggregate.ConfidenceScore)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_PPEBehavioralMonitoring_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="PPEBehavioralMonitoring"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring Hydrate(PPEBehavioralMonitoringRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring>();

        AggregateReflectionHydrator.SetProperty(agg, "PPEBehavioralMonitoringId", PPEBehavioralMonitoringId.FromGuid(row.PPEBehavioralMonitoringId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PPEBehavioralMonitoringStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "SensorId", row.SensorId);
        AggregateReflectionHydrator.SetProperty(agg, "ZoneCode", row.ZoneCode);
        AggregateReflectionHydrator.SetProperty(agg, "DetectedEventType", row.DetectedEventType);
        AggregateReflectionHydrator.SetProperty(agg, "ConfidenceScore", row.ConfidenceScore);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPPEBehavioralMonitoringDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PPEBehavioralMonitoring_GetById</c>/<c>usp_PPEBehavioralMonitoring_GetAllActive</c>.</summary>
    private sealed record PPEBehavioralMonitoringRow
    {
        public required Guid PPEBehavioralMonitoringId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string SensorId { get; init; }
        public required string ZoneCode { get; init; }
        public required string DetectedEventType { get; init; }
        public required decimal ConfidenceScore { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
