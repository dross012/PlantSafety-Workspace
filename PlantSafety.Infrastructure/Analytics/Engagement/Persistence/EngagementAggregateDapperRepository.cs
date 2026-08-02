// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Infrastructure
//  File:           EngagementAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IEngagementRepository (Domain repository
//                  interface for the Engagement aggregate root). All DB access via stored
//                  procedures in the [Analytics] schema, targeting the new
//                  [Analytics].[EngagementRecords] table. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Engagement exposes only a private "for
//                  rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied EngagementSpecification in-memory over the full active
//                  candidate set, since the interface carries no tenant/site scoping parameter to
//                  push down to SQL (mirrors PermitsDapperRepository). Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write. GetByIdAsync
//                  is tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from
//                  day one. Named "EngagementAggregateDapperRepository" (not
//                  "EngagementDapperRepository") to avoid an ambiguous-reference compile error
//                  against the aggregate type PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement
//                  (same simple name as the "Engagement" namespace segment this file lives under),
//                  exactly like the SafetyEvents agents' identical precedent. Mirrors
//                  PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Analytics.Engagement.Aggregates;
using PlantSafety.Domain.Analytics.Engagement.Enums;
using PlantSafety.Domain.Analytics.Engagement.Events;
using PlantSafety.Domain.Analytics.Engagement.Interfaces;
using PlantSafety.Domain.Analytics.Engagement.Specifications;
using PlantSafety.Domain.Analytics.Engagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Analytics.Engagement.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Engagement"/> aggregate root.</summary>
public sealed class EngagementAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EngagementAggregateDapperRepository> logger)
    : IEngagementRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement?> GetByIdAsync(EngagementId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Engagement {EngagementId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EngagementId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EngagementRow>(
            "[Analytics].[usp_EngagementRecords_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement>> ListAsync(EngagementSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Engagement via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<EngagementRow>(
            "[Analytics].[usp_EngagementRecords_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Engagement {EngagementId}", aggregate.EngagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EngagementId", aggregate.EngagementId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("WorkerId", aggregate.WorkerId.Value)
            .AddDecimal("EngagementScore", aggregate.EngagementScore)
            .AddString("SurveyPeriod", aggregate.SurveyPeriod, 50)
            .AddString("ParticipationChannel", aggregate.ParticipationChannel, 100)
            .AddInt("ResponseCount", aggregate.ResponseCount)
            .AddDateTime("LastEngagedAtUtc", aggregate.LastEngagedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Analytics].[usp_EngagementRecords_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Engagement {EngagementId}", aggregate.EngagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EngagementId", aggregate.EngagementId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("WorkerId", aggregate.WorkerId.Value)
            .AddDecimal("EngagementScore", aggregate.EngagementScore)
            .AddString("SurveyPeriod", aggregate.SurveyPeriod, 50)
            .AddString("ParticipationChannel", aggregate.ParticipationChannel, 100)
            .AddInt("ResponseCount", aggregate.ResponseCount)
            .AddDateTime("LastEngagedAtUtc", aggregate.LastEngagedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_EngagementRecords_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Analytics].[usp_EngagementRecords_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Engagement {aggregate.EngagementId.Value} was modified by another process or the ETag is stale.",
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

    /// <summary>Reconstructs an <see cref="Engagement"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement Hydrate(EngagementRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.EngagementId), EngagementId.FromGuid(row.EngagementId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.Status), Enum.Parse<EngagementStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.WorkerId), new UserId(row.WorkerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.EngagementScore), row.EngagementScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.SurveyPeriod), row.SurveyPeriod);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.ParticipationChannel), row.ParticipationChannel);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.ResponseCount), row.ResponseCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.LastEngagedAtUtc), row.LastEngagedAtUtc.HasValue ? UtcTimestamp.From(row.LastEngagedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IEngagementDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_EngagementRecords_GetById</c>/<c>usp_EngagementRecords_GetAllActive</c>.</summary>
    private sealed record EngagementRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_EngagementRecords_GetById</c>/<c>usp_EngagementRecords_GetAllActive</c>. Not
        /// consumed by aggregate hydration -- callers obtain the ETag for a subsequent write from
        /// a prior single-record read (see the Application-layer EngagementItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid EngagementId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid WorkerId { get; init; }
        public required decimal EngagementScore { get; init; }
        public required string SurveyPeriod { get; init; }
        public required string ParticipationChannel { get; init; }
        public required int ResponseCount { get; init; }
        public DateTime? LastEngagedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
