// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Infrastructure
//  File:           PredictiveSafetyAlertAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer write-side
//                  PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces
//                  .IPredictiveSafetyAlertRepository for the PredictiveSafetyAlert aggregate root.
//                  All DB access via stored procedures in the [Intelligence] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because PredictiveSafetyAlert
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag from day one. Both AddAsync AND UpdateAsync enqueue domain
//                  events to the outbox after their write. Named
//                  "PredictiveSafetyAlertAggregateDapperRepository" (rather than
//                  "PredictiveSafetyDapperRepository") to avoid colliding with the pre-existing
//                  read-list PredictiveSafetyDapperRepository already declared in the sibling
//                  Persistence namespace (backs the unrelated, untouched date-range
//                  GetPredictiveSafetyQuery/usp_PredictiveSafety_GetPredictions read path). Mirrors
//                  PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence
//                  .RCAAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Enums;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Events;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces;
using PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PredictiveSafetyAlertAggregate = PlantSafety.Domain.Intelligence.PredictiveSafety.Aggregates.PredictiveSafetyAlert;

namespace PlantSafety.Infrastructure.Intelligence.PredictiveSafety.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PredictiveSafetyAlertAggregate"/> aggregate root.</summary>
public sealed class PredictiveSafetyAlertAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PredictiveSafetyAlertAggregateDapperRepository> logger)
    : IPredictiveSafetyAlertRepository
{
    /// <inheritdoc/>
    public async Task<PredictiveSafetyAlertAggregate?> GetByIdAsync(PredictiveSafetyAlertId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PredictiveSafetyAlert {PredictiveSafetyAlertId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveSafetyAlertId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PredictiveSafetyAlertRow>(
            "[Intelligence].[usp_PredictiveSafetyAlert_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PredictiveSafetyAlertAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PredictiveSafetyAlert {PredictiveSafetyAlertId}", aggregate.PredictiveSafetyAlertId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveSafetyAlertId", aggregate.PredictiveSafetyAlertId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("RiskCategory", aggregate.RiskCategory, 100)
            .AddDouble("PredictedRiskScore", aggregate.PredictedRiskScore)
            .AddString("ModelVersion", aggregate.ModelVersion, 50)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("PredictedAtUtc", aggregate.PredictedAtUtc.Value)
            .AddGuid("GeneratedBy", aggregate.GeneratedBy.Value)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddDateTime("AcknowledgedAtUtc", aggregate.AcknowledgedAtUtc?.Value)
            .AddGuid("ResolvedBy", aggregate.ResolvedBy?.Value)
            .AddString("ResolutionNotes", aggregate.ResolutionNotes, size: -1)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Intelligence].[usp_PredictiveSafetyAlert_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PredictiveSafetyAlertAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PredictiveSafetyAlert {PredictiveSafetyAlertId}", aggregate.PredictiveSafetyAlertId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveSafetyAlertId", aggregate.PredictiveSafetyAlertId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("RiskCategory", aggregate.RiskCategory, 100)
            .AddDouble("PredictedRiskScore", aggregate.PredictedRiskScore)
            .AddString("ModelVersion", aggregate.ModelVersion, 50)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddDateTime("AcknowledgedAtUtc", aggregate.AcknowledgedAtUtc?.Value)
            .AddGuid("ResolvedBy", aggregate.ResolvedBy?.Value)
            .AddString("ResolutionNotes", aggregate.ResolutionNotes, size: -1)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("ModifiedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PredictiveSafetyAlert_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Intelligence].[usp_PredictiveSafetyAlert_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PredictiveSafetyAlert {aggregate.PredictiveSafetyAlertId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PredictiveSafetyAlertAggregate"/> aggregate from its flat read-model row.</summary>
    private static PredictiveSafetyAlertAggregate Hydrate(PredictiveSafetyAlertRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PredictiveSafetyAlertAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.PredictiveSafetyAlertId), PredictiveSafetyAlertId.FromGuid(row.PredictiveSafetyAlertId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.Status), Enum.Parse<PredictiveSafetyAlertStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.RiskCategory), row.RiskCategory);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.PredictedRiskScore), row.PredictedRiskScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.ModelVersion), row.ModelVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.PredictedAtUtc), UtcTimestamp.From(row.PredictedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.GeneratedBy), new UserId(row.GeneratedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.AcknowledgedBy), row.AcknowledgedBy.HasValue ? new UserId(row.AcknowledgedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.AcknowledgedAtUtc), row.AcknowledgedAtUtc.HasValue ? UtcTimestamp.From(row.AcknowledgedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.ResolvedBy), row.ResolvedBy.HasValue ? new UserId(row.ResolvedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.ResolutionNotes), row.ResolutionNotes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.ResolvedAtUtc), row.ResolvedAtUtc.HasValue ? UtcTimestamp.From(row.ResolvedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveSafetyAlertAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPredictiveSafetyAlertDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PredictiveSafetyAlert_GetById</c>.</summary>
    private sealed record PredictiveSafetyAlertRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PredictiveSafetyAlert_GetById</c>. Not consumed by aggregate hydration --
        /// callers obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer PredictiveSafetyAlertDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PredictiveSafetyAlertId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string RiskCategory { get; init; }
        public required double PredictedRiskScore { get; init; }
        public string? ModelVersion { get; init; }
        public required string Status { get; init; }
        public required DateTime PredictedAtUtc { get; init; }
        public required Guid GeneratedBy { get; init; }
        public Guid? AcknowledgedBy { get; init; }
        public DateTime? AcknowledgedAtUtc { get; init; }
        public Guid? ResolvedBy { get; init; }
        public string? ResolutionNotes { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
