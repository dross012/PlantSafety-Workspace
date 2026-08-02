// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Infrastructure
//  File:           AIInvestigationAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAIInvestigationRepository (Domain write-side
//                  repository interface for the AIInvestigation aggregate root). All DB access via
//                  stored procedures in the [Intelligence] schema, against the singular
//                  [Intelligence].[AIInvestigation] table (distinct from the pre-existing,
//                  unrelated read-only [Intelligence].[AllInvestigations] table backing
//                  IAllInvestigationsRepository). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because AIInvestigation exposes only a private "for
//                  rehydration only" constructor and private-setter properties. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from day
//                  one. Both AddAsync AND UpdateAsync enqueue domain events to the outbox after
//                  their write. Mirrors
//                  PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence
//                  .RootCauseAnalysisAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Intelligence.AIInvestigations.Enums;
using PlantSafety.Domain.Intelligence.AIInvestigations.Events;
using PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces;
using PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using AIInvestigationAggregate = PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates.AIInvestigation;

namespace PlantSafety.Infrastructure.Intelligence.AIInvestigations.Persistence;

/// <summary>Dapper-backed repository for the <see cref="AIInvestigationAggregate"/> aggregate root.</summary>
public sealed class AIInvestigationAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AIInvestigationAggregateDapperRepository> logger)
    : IAIInvestigationRepository
{
    /// <inheritdoc/>
    public async Task<AIInvestigationAggregate?> GetByIdAsync(AIInvestigationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AIInvestigation {AIInvestigationId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AIInvestigationId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AIInvestigationRow>(
            "[Intelligence].[usp_AIInvestigation_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(AIInvestigationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add AIInvestigation {AIInvestigationId}", aggregate.AIInvestigationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AIInvestigationId", aggregate.AIInvestigationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("IncidentReference", aggregate.IncidentReference, 100)
            .AddDecimal("AiConfidenceScore", (decimal)aggregate.AiConfidenceScore)
            .AddString("Summary", aggregate.Summary, size: -1)
            .AddGuid("AssignedTo", aggregate.AssignedTo.Value)
            .AddDateTime("OpenedAtUtc", aggregate.OpenedAtUtc.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Intelligence].[usp_AIInvestigation_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AIInvestigationAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AIInvestigation {AIInvestigationId}", aggregate.AIInvestigationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AIInvestigationId", aggregate.AIInvestigationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Summary", aggregate.Summary, size: -1)
            .AddGuid("ReviewStartedBy", aggregate.ReviewStartedBy?.Value)
            .AddDateTime("ReviewStartedAtUtc", aggregate.ReviewStartedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddString("ResolutionSummary", aggregate.ResolutionSummary, size: -1)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_AIInvestigation_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Intelligence].[usp_AIInvestigation_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"AIInvestigation {aggregate.AIInvestigationId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="AIInvestigationAggregate"/> aggregate from its flat read-model row.</summary>
    private static AIInvestigationAggregate Hydrate(AIInvestigationRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<AIInvestigationAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.AIInvestigationId), AIInvestigationId.FromGuid(row.AIInvestigationId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.Status), Enum.Parse<AIInvestigationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.IncidentReference), row.IncidentReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.AiConfidenceScore), row.AiConfidenceScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.Summary), row.Summary);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.AssignedTo), new UserId(row.AssignedTo));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.OpenedAtUtc), UtcTimestamp.From(row.OpenedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.ReviewStartedBy), row.ReviewStartedBy.HasValue ? new UserId(row.ReviewStartedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.ReviewStartedAtUtc), row.ReviewStartedAtUtc.HasValue ? UtcTimestamp.From(row.ReviewStartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.ResolutionSummary), row.ResolutionSummary);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AIInvestigationAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAIInvestigationDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AIInvestigation_GetById</c>.</summary>
    private sealed record AIInvestigationRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string). Not
        /// consumed by aggregate hydration -- callers obtain the ETag for a subsequent write from
        /// a prior single-record read.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid AIInvestigationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string IncidentReference { get; init; }
        public required double AiConfidenceScore { get; init; }
        public string? Summary { get; init; }
        public required Guid AssignedTo { get; init; }
        public required DateTime OpenedAtUtc { get; init; }
        public Guid? ReviewStartedBy { get; init; }
        public DateTime? ReviewStartedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public string? ResolutionSummary { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
