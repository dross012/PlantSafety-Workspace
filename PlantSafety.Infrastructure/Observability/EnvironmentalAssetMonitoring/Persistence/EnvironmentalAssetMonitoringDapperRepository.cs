// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Infrastructure
//  File:           EnvironmentalAssetMonitoringDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IEnvironmentalAssetMonitoringRepository (Domain repository
//                  interface for the EnvironmentalAssetMonitoring aggregate root). All DB access via stored
//                  procedures in the [Observability] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because EnvironmentalAssetMonitoring exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates;
using PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Enums;
using PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Events;
using PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Interfaces;
using PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Specifications;
using PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Observability.EnvironmentalAssetMonitoring.Persistence;

/// <summary>Dapper-backed repository for the <see cref="EnvironmentalAssetMonitoring"/> aggregate root.</summary>
public sealed class EnvironmentalAssetMonitoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EnvironmentalAssetMonitoringDapperRepository> logger)
    : IEnvironmentalAssetMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring?> GetByIdAsync(EnvironmentalAssetMonitoringId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EnvironmentalAssetMonitoring {EnvironmentalAssetMonitoringId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("EnvironmentalAssetMonitoringId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EnvironmentalAssetMonitoringRow>(
            "[Observability].[usp_EnvironmentalAssetMonitoring_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring>> ListAsync(EnvironmentalAssetMonitoringSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List EnvironmentalAssetMonitoring via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<EnvironmentalAssetMonitoringRow>(
            "[Observability].[usp_EnvironmentalAssetMonitoring_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add EnvironmentalAssetMonitoring {EnvironmentalAssetMonitoringId}", aggregate.EnvironmentalAssetMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalAssetMonitoringId", aggregate.EnvironmentalAssetMonitoringId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SensorId", aggregate.SensorId, 400)
            .AddString("MeasurementType", aggregate.MeasurementType, 400)
            .AddDecimal("Value", aggregate.Value)
            .AddBool("ThresholdExceeded", aggregate.ThresholdExceeded)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_EnvironmentalAssetMonitoring_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update EnvironmentalAssetMonitoring {EnvironmentalAssetMonitoringId}", aggregate.EnvironmentalAssetMonitoringId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalAssetMonitoringId", aggregate.EnvironmentalAssetMonitoringId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SensorId", aggregate.SensorId, 400)
            .AddString("MeasurementType", aggregate.MeasurementType, 400)
            .AddDecimal("Value", aggregate.Value)
            .AddBool("ThresholdExceeded", aggregate.ThresholdExceeded)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_EnvironmentalAssetMonitoring_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="EnvironmentalAssetMonitoring"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring Hydrate(EnvironmentalAssetMonitoringRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring>();

        AggregateReflectionHydrator.SetProperty(agg, "EnvironmentalAssetMonitoringId", EnvironmentalAssetMonitoringId.FromGuid(row.EnvironmentalAssetMonitoringId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<EnvironmentalAssetMonitoringStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "SensorId", row.SensorId);
        AggregateReflectionHydrator.SetProperty(agg, "MeasurementType", row.MeasurementType);
        AggregateReflectionHydrator.SetProperty(agg, "Value", row.Value);
        AggregateReflectionHydrator.SetProperty(agg, "ThresholdExceeded", row.ThresholdExceeded);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IEnvironmentalAssetMonitoringDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_EnvironmentalAssetMonitoring_GetById</c>/<c>usp_EnvironmentalAssetMonitoring_GetAllActive</c>.</summary>
    private sealed record EnvironmentalAssetMonitoringRow
    {
        public required Guid EnvironmentalAssetMonitoringId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string SensorId { get; init; }
        public required string MeasurementType { get; init; }
        public required decimal Value { get; init; }
        public required bool ThresholdExceeded { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
