// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IEnvironmentalHealthRepository (Domain
//                  repository interface for the EnvironmentalHealth aggregate root). All DB
//                  access via stored procedures in the [EnvironmentalHealth] schema, against
//                  [EnvironmentalHealth].[EnvironmentalHealthAggregate] (named to avoid
//                  ambiguity with the pre-existing, dead EnvironmentalHealthReadings table/
//                  usp_EnvironmentalHealth_GetById/_List procs that live under this same module
//                  folder, and with the entirely separate, LIVE Features.EnvironmentalHealth
//                  "reading" vertical). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because EnvironmentalHealth exposes only a
//                  private "for rehydration only" constructor and private-setter properties.
//                  Both AddAsync AND UpdateAsync enqueue domain events to the outbox after their
//                  write. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.EnvironmentalHealth.Aggregates;
using PlantSafety.Domain.EnvironmentalHealth.Enums;
using PlantSafety.Domain.EnvironmentalHealth.Events;
using PlantSafety.Domain.EnvironmentalHealth.Repositories;
using PlantSafety.Domain.EnvironmentalHealth.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.Persistence;

/// <summary>Dapper-backed repository for the <see cref="EnvironmentalHealth"/> aggregate root.</summary>
public sealed class EnvironmentalHealthAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EnvironmentalHealthAggregateDapperRepository> logger)
    : IEnvironmentalHealthRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth?> GetByIdAsync(EnvironmentalHealthId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EnvironmentalHealth {EnvironmentalHealthId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalHealthId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EnvironmentalHealthRow>(
            "[EnvironmentalHealth].[usp_EnvironmentalHealthAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add EnvironmentalHealth {EnvironmentalHealthId}", aggregate.EnvironmentalHealthId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalHealthId", aggregate.EnvironmentalHealthId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("MonitoringType", aggregate.MonitoringType, 100)
            .AddDecimal("MeasuredValue", aggregate.MeasuredValue)
            .AddString("Unit", aggregate.Unit, 50)
            .AddDecimal("Threshold", aggregate.Threshold)
            .AddBool("ThresholdExceeded", aggregate.ThresholdExceeded)
            .AddString("EvaluatedStatus", aggregate.EvaluatedStatus, 50)
            .AddDateTime("RecordedAtUtc", aggregate.RecordedAtUtc.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[EnvironmentalHealth].[usp_EnvironmentalHealthAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update EnvironmentalHealth {EnvironmentalHealthId}", aggregate.EnvironmentalHealthId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalHealthId", aggregate.EnvironmentalHealthId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("MonitoringType", aggregate.MonitoringType, 100)
            .AddDecimal("MeasuredValue", aggregate.MeasuredValue)
            .AddString("Unit", aggregate.Unit, 50)
            .AddDecimal("Threshold", aggregate.Threshold)
            .AddBool("ThresholdExceeded", aggregate.ThresholdExceeded)
            .AddString("EvaluatedStatus", aggregate.EvaluatedStatus, 50)
            .AddDateTime("RecordedAtUtc", aggregate.RecordedAtUtc.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_EnvironmentalHealthAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[EnvironmentalHealth].[usp_EnvironmentalHealthAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"EnvironmentalHealth {aggregate.EnvironmentalHealthId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="EnvironmentalHealth"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth Hydrate(EnvironmentalHealthRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.EnvironmentalHealthId), EnvironmentalHealthId.FromGuid(row.EnvironmentalHealthId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.Status), Enum.Parse<EnvironmentalHealthStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.MonitoringType), row.MonitoringType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.MeasuredValue), row.MeasuredValue);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.Unit), row.Unit);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.Threshold), row.Threshold);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.ThresholdExceeded), row.ThresholdExceeded);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.EvaluatedStatus), row.EvaluatedStatus);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.RecordedAtUtc), row.RecordedAtUtc.HasValue ? UtcTimestamp.From(row.RecordedAtUtc.Value) : UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IEnvironmentalHealthDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_EnvironmentalHealthAggregate_GetById</c>.</summary>
    private sealed record EnvironmentalHealthRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_EnvironmentalHealthAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid EnvironmentalHealthId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string MonitoringType { get; init; }
        public decimal? MeasuredValue { get; init; }
        public required string Unit { get; init; }
        public decimal Threshold { get; init; }
        public bool ThresholdExceeded { get; init; }
        public required string EvaluatedStatus { get; init; }
        public DateTime? RecordedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
