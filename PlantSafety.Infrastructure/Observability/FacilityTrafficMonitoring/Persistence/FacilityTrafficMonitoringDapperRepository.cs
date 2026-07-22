// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Infrastructure
//  File:           FacilityTrafficMonitoringDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IFacilityTrafficMonitoringRepository (Domain repository
//                  interface for the FacilityTrafficMonitoring aggregate root). All DB access via stored
//                  procedures in the [Observability] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because FacilityTrafficMonitoring exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates;
using PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Enums;
using PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Events;
using PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Interfaces;
using PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Specifications;
using PlantSafety.Domain.Observability.FacilityTrafficMonitoring.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Observability.FacilityTrafficMonitoring.Persistence;

/// <summary>Dapper-backed repository for the <see cref="FacilityTrafficMonitoring"/> aggregate root.</summary>
public sealed class FacilityTrafficMonitoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<FacilityTrafficMonitoringDapperRepository> logger)
    : IFacilityTrafficMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring?> GetByIdAsync(FacilityTrafficMonitoringId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for FacilityTrafficMonitoring {FacilityTrafficMonitoringId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("FacilityTrafficMonitoringId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<FacilityTrafficMonitoringRow>(
            "[Observability].[usp_FacilityTrafficMonitoring_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring>> ListAsync(FacilityTrafficMonitoringSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List FacilityTrafficMonitoring via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<FacilityTrafficMonitoringRow>(
            "[Observability].[usp_FacilityTrafficMonitoring_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add FacilityTrafficMonitoring {FacilityTrafficMonitoringId}", aggregate.FacilityTrafficMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FacilityTrafficMonitoringId", aggregate.FacilityTrafficMonitoringId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ZoneCode", aggregate.ZoneCode, 400)
            .AddString("EventType", aggregate.EventType, 400)
            .AddDecimal("SeverityScore", aggregate.SeverityScore)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_FacilityTrafficMonitoring_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update FacilityTrafficMonitoring {FacilityTrafficMonitoringId}", aggregate.FacilityTrafficMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FacilityTrafficMonitoringId", aggregate.FacilityTrafficMonitoringId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ZoneCode", aggregate.ZoneCode, 400)
            .AddString("EventType", aggregate.EventType, 400)
            .AddDecimal("SeverityScore", aggregate.SeverityScore)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_FacilityTrafficMonitoring_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="FacilityTrafficMonitoring"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring Hydrate(FacilityTrafficMonitoringRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring>();

        AggregateReflectionHydrator.SetProperty(agg, "FacilityTrafficMonitoringId", FacilityTrafficMonitoringId.FromGuid(row.FacilityTrafficMonitoringId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<FacilityTrafficMonitoringStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "ZoneCode", row.ZoneCode);
        AggregateReflectionHydrator.SetProperty(agg, "EventType", row.EventType);
        AggregateReflectionHydrator.SetProperty(agg, "SeverityScore", row.SeverityScore);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IFacilityTrafficMonitoringDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_FacilityTrafficMonitoring_GetById</c>/<c>usp_FacilityTrafficMonitoring_GetAllActive</c>.</summary>
    private sealed record FacilityTrafficMonitoringRow
    {
        public required Guid FacilityTrafficMonitoringId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ZoneCode { get; init; }
        public required string EventType { get; init; }
        public required decimal SeverityScore { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
