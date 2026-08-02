// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Infrastructure
//  File:           ScoreCardsAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IScoreCardsRepository (Domain repository
//                  interface for the ScoreCards aggregate root). All DB access via stored
//                  procedures in the [Analytics] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because ScoreCards exposes only a private "for
//                  rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied ScoreCardsSpecification in-memory over the full active
//                  candidate set, since the interface carries no tenant/site scoping parameter to
//                  push down to SQL (mirrors PermitsDapperRepository). Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write. GetByIdAsync
//                  is tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from
//                  day one (this aggregate had no prior write path to retrofit). Named
//                  ScoreCardsAggregateDapperRepository (rather than bare ScoreCardsDapperRepository)
//                  to avoid a same-simple-name collision with the pre-existing, unregistered
//                  ScoreCardsRepository class already declared in this same namespace
//                  ([Analytics].ScoreCards.Persistence) -- mirrors the naming precedent applied by
//                  the SafetyEvents build-out agents for an identical namespace-collision. Mirrors
//                  PermitsDapperRepository.cs. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Analytics.ScoreCards.Enums;
using PlantSafety.Domain.Analytics.ScoreCards.Events;
using PlantSafety.Domain.Analytics.ScoreCards.Interfaces;
using PlantSafety.Domain.Analytics.ScoreCards.Specifications;
using PlantSafety.Domain.Analytics.ScoreCards.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using ScoreCardsAggregate = PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ScoreCardsAggregate"/> aggregate root.</summary>
public sealed class ScoreCardsAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ScoreCardsAggregateDapperRepository> logger)
    : IScoreCardsRepository
{
    /// <inheritdoc/>
    public async Task<ScoreCardsAggregate?> GetByIdAsync(ScoreCardsId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ScoreCards {ScoreCardsId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ScoreCardsId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ScoreCardsRow>(
            "[Analytics].[usp_ScoreCardsAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScoreCardsAggregate>> ListAsync(ScoreCardsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List ScoreCards via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<ScoreCardsRow>(
            "[Analytics].[usp_ScoreCardsAggregate_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<ScoreCardsAggregate>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(ScoreCardsAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ScoreCards {ScoreCardsId}", aggregate.ScoreCardsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ScoreCardsId", aggregate.ScoreCardsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ScoreCardName", aggregate.ScoreCardName, 400)
            .AddString("ReportingPeriod", aggregate.ReportingPeriod, 50)
            .AddString("KpiCategory", aggregate.KpiCategory, 200)
            .AddDecimal("PerformanceIndex", aggregate.PerformanceIndex)
            .AddGuid("GeneratedForSiteId", aggregate.GeneratedForSiteId)
            .AddDateTime("GeneratedAtUtc", aggregate.GeneratedAtUtc.Value)
            .AddGuid("PublishedByUserId", aggregate.PublishedByUserId?.Value)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddGuid("FinalizedByUserId", aggregate.FinalizedByUserId?.Value)
            .AddDateTime("FinalizedAtUtc", aggregate.FinalizedAtUtc?.Value)
            .AddGuid("ClosedByUserId", aggregate.ClosedByUserId?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Analytics].[usp_ScoreCardsAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(ScoreCardsAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ScoreCards {ScoreCardsId}", aggregate.ScoreCardsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ScoreCardsId", aggregate.ScoreCardsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ScoreCardName", aggregate.ScoreCardName, 400)
            .AddString("ReportingPeriod", aggregate.ReportingPeriod, 50)
            .AddString("KpiCategory", aggregate.KpiCategory, 200)
            .AddDecimal("PerformanceIndex", aggregate.PerformanceIndex)
            .AddGuid("GeneratedForSiteId", aggregate.GeneratedForSiteId)
            .AddDateTime("GeneratedAtUtc", aggregate.GeneratedAtUtc.Value)
            .AddGuid("PublishedByUserId", aggregate.PublishedByUserId?.Value)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .AddGuid("FinalizedByUserId", aggregate.FinalizedByUserId?.Value)
            .AddDateTime("FinalizedAtUtc", aggregate.FinalizedAtUtc?.Value)
            .AddGuid("ClosedByUserId", aggregate.ClosedByUserId?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_ScoreCardsAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Analytics].[usp_ScoreCardsAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ScoreCards {aggregate.ScoreCardsId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring: UpdateAsync must enqueue too, not just AddAsync -- otherwise every
        // post-creation transition (Publish, Finalize, Close, Archive) would silently drop its
        // domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="ScoreCardsAggregate"/> aggregate from its flat read-model row.</summary>
    private static ScoreCardsAggregate Hydrate(ScoreCardsRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ScoreCardsAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.ScoreCardsId), ScoreCardsId.FromGuid(row.ScoreCardsId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.Status), Enum.Parse<ScoreCardsStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.ScoreCardName), row.ScoreCardName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.ReportingPeriod), row.ReportingPeriod);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.KpiCategory), row.KpiCategory);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.PerformanceIndex), row.PerformanceIndex);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.GeneratedForSiteId), row.GeneratedForSiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.GeneratedAtUtc), UtcTimestamp.From(row.GeneratedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.PublishedByUserId), row.PublishedByUserId.HasValue ? new UserId(row.PublishedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.PublishedAtUtc), row.PublishedAtUtc.HasValue ? UtcTimestamp.From(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.FinalizedByUserId), row.FinalizedByUserId.HasValue ? new UserId(row.FinalizedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.FinalizedAtUtc), row.FinalizedAtUtc.HasValue ? UtcTimestamp.From(row.FinalizedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.ClosedByUserId), row.ClosedByUserId.HasValue ? new UserId(row.ClosedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ScoreCardsAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IScoreCardsDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ScoreCardsAggregate_GetById</c>/<c>usp_ScoreCardsAggregate_GetAllActive</c>.</summary>
    private sealed record ScoreCardsRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_ScoreCardsAggregate_GetById</c>/<c>usp_ScoreCardsAggregate_GetAllActive</c>.
        /// Not consumed by aggregate hydration -- callers obtain the ETag for a subsequent write
        /// from a prior single-record read (see the Application-layer ScoreCardsItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ScoreCardsId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ScoreCardName { get; init; }
        public required string ReportingPeriod { get; init; }
        public required string KpiCategory { get; init; }
        public required decimal PerformanceIndex { get; init; }
        public Guid? GeneratedForSiteId { get; init; }
        public required DateTime GeneratedAtUtc { get; init; }
        public Guid? PublishedByUserId { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
        public Guid? FinalizedByUserId { get; init; }
        public DateTime? FinalizedAtUtc { get; init; }
        public Guid? ClosedByUserId { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
