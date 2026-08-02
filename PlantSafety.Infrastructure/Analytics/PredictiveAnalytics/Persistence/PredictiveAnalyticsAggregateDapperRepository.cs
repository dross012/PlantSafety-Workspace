// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Infrastructure
//  File:           PredictiveAnalyticsAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPredictiveAnalyticsRepository (Domain
//                  repository interface for the PredictiveAnalytics aggregate root). All DB
//                  access via stored procedures in the [Analytics] schema, targeting the same
//                  [Analytics].[PredictiveAnalytics] table used by the pre-existing read-only
//                  IPredictiveAnalyticsRepository (Application-layer, distinct namespace) --
//                  additively extended (see PredictiveAnalytics.sql) rather than duplicated.
//                  Aggregate reconstruction uses AggregateReflectionHydrator because
//                  PredictiveAnalytics exposes only a private "for rehydration only" constructor
//                  and private-setter properties. ListAsync evaluates the supplied
//                  PredictiveAnalyticsSpecification in-memory over the full active candidate set
//                  (mirrors PermitsDapperRepository). Both AddAsync AND UpdateAsync enqueue domain
//                  events to the outbox after their write. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag from day one. Named
//                  "PredictiveAnalyticsAggregateDapperRepository" (not
//                  "PredictiveAnalyticsDapperRepository", which is already taken by the
//                  pre-existing Application-read-side class in this same namespace, and to avoid
//                  the same simple-name-as-namespace-segment ambiguity risk flagged for
//                  Engagement) to coexist alongside PredictiveAnalyticsDapperRepository (read) and
//                  the dead PredictiveAnalyticsRepository scaffold. Mirrors PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Enums;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Events;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Specifications;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PredictiveAnalytics"/> aggregate root.</summary>
public sealed class PredictiveAnalyticsAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PredictiveAnalyticsAggregateDapperRepository> logger)
    : IPredictiveAnalyticsRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics?> GetByIdAsync(PredictiveAnalyticsId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PredictiveAnalytics {PredictiveAnalyticsId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictionId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PredictiveAnalyticsRow>(
            "[Analytics].[usp_PredictiveAnalytics_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics>> ListAsync(PredictiveAnalyticsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PredictiveAnalytics via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PredictiveAnalyticsRow>(
            "[Analytics].[usp_PredictiveAnalytics_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PredictiveAnalytics {PredictiveAnalyticsId}", aggregate.PredictiveAnalyticsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictionId", aggregate.PredictiveAnalyticsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ModelName", aggregate.ModelName, 200)
            .AddString("PredictionType", aggregate.PredictionType, 200)
            .AddGuid("TargetEntityId", aggregate.TargetEntityId)
            .AddDecimal("Score", aggregate.PredictedRiskScore)
            .AddDecimal("ConfidenceLevel", aggregate.ConfidenceLevel)
            .AddInt("PredictionHorizonDays", aggregate.PredictionHorizonDays)
            .AddDateTime("GeneratedAtUtc", aggregate.GeneratedAtUtc.Value)
            .AddGuid("GeneratedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Analytics].[usp_PredictiveAnalytics_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring: enqueue any domain events raised by the aggregate during this write so
        // TransactionalUnitOfWork.CommitAsync (invoked by TransactionBehavior after the handler
        // returns) can flush them to [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PredictiveAnalytics {PredictiveAnalyticsId}", aggregate.PredictiveAnalyticsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictionId", aggregate.PredictiveAnalyticsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ModelName", aggregate.ModelName, 200)
            .AddString("PredictionType", aggregate.PredictionType, 200)
            .AddGuid("TargetEntityId", aggregate.TargetEntityId)
            .AddDecimal("Score", aggregate.PredictedRiskScore)
            .AddDecimal("ConfidenceLevel", aggregate.ConfidenceLevel)
            .AddInt("PredictionHorizonDays", aggregate.PredictionHorizonDays)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PredictiveAnalytics_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Analytics].[usp_PredictiveAnalytics_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PredictiveAnalytics {aggregate.PredictiveAnalyticsId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring: enqueue any domain events raised during this lifecycle transition.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PredictiveAnalytics"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics Hydrate(PredictiveAnalyticsRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.PredictiveAnalyticsId), PredictiveAnalyticsId.FromGuid(row.PredictionId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.Status), Enum.Parse<PredictiveAnalyticsStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.ModelName), row.ModelName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.PredictionType), row.PredictionType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.TargetEntityId), row.TargetEntityId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.PredictedRiskScore), row.Score);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.ConfidenceLevel), row.ConfidenceLevel);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.PredictionHorizonDays), row.PredictionHorizonDays);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.GeneratedAtUtc), UtcTimestamp.From(row.GeneratedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.CreatedAtUtc), UtcTimestamp.From(row.GeneratedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.CreatedBy), new UserId(row.GeneratedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPredictiveAnalyticsDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PredictiveAnalytics_GetById</c>/<c>usp_PredictiveAnalytics_GetAllActive</c>.</summary>
    private sealed record PredictiveAnalyticsRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PredictiveAnalytics_GetById</c>/<c>usp_PredictiveAnalytics_GetAllActive</c>.
        /// Not consumed by aggregate hydration -- callers obtain the ETag for a subsequent write
        /// from a prior single-record read (see the Application-layer PredictiveAnalyticsItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PredictionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ModelName { get; init; }
        public required string PredictionType { get; init; }
        public Guid? TargetEntityId { get; init; }
        public required decimal Score { get; init; }
        public required decimal ConfidenceLevel { get; init; }
        public required int PredictionHorizonDays { get; init; }
        public required DateTime GeneratedAtUtc { get; init; }
        public required Guid GeneratedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
