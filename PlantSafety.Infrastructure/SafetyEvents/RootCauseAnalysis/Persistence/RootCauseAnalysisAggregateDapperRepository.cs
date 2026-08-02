// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Infrastructure
//  File:           RootCauseAnalysisAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IRootCauseAnalysisRepository (Domain
//                  write-side repository interface for the RootCauseAnalysis aggregate root). All
//                  DB access via stored procedures in the [SafetyEvents] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because RootCauseAnalysis
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag from day one. Both AddAsync AND UpdateAsync enqueue
//                  domain events to the outbox after their write. Named
//                  "...AggregateDapperRepository" (rather than "RootCauseAnalysisDapperRepository")
//                  to avoid colliding with the pre-existing read-list
//                  RootCauseAnalysisDapperRepository already declared in this same Persistence
//                  namespace. Mirrors
//                  PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence
//                  .PredictiveMaintenanceAlertDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Enums;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Events;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using RootCauseAnalysisAggregate = PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates.RootCauseAnalysis;

namespace PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence;

/// <summary>Dapper-backed repository for the <see cref="RootCauseAnalysisAggregate"/> aggregate root.</summary>
public sealed class RootCauseAnalysisAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RootCauseAnalysisAggregateDapperRepository> logger)
    : IRootCauseAnalysisRepository
{
    /// <inheritdoc/>
    public async Task<RootCauseAnalysisAggregate?> GetByIdAsync(RootCauseAnalysisId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RootCauseAnalysis {RootCauseAnalysisId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RootCauseAnalysisId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<RootCauseAnalysisRow>(
            "[SafetyEvents].[usp_RootCauseAnalysis_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(RootCauseAnalysisAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add RootCauseAnalysis {RootCauseAnalysisId}", aggregate.RootCauseAnalysisId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RootCauseAnalysisId", aggregate.RootCauseAnalysisId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("RelatedEventId", aggregate.RelatedEventId)
            .AddString("Methodology", aggregate.Methodology, 50)
            .AddGuid("LeadAnalyst", aggregate.LeadAnalyst.Value)
            .AddGuid("SubmittedBy", aggregate.SubmittedBy?.Value)
            .AddDateTime("SubmittedAtUtc", aggregate.SubmittedAtUtc?.Value)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_RootCauseAnalysis_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(RootCauseAnalysisAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update RootCauseAnalysis {RootCauseAnalysisId}", aggregate.RootCauseAnalysisId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RootCauseAnalysisId", aggregate.RootCauseAnalysisId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("RelatedEventId", aggregate.RelatedEventId)
            .AddString("Methodology", aggregate.Methodology, 50)
            .AddGuid("SubmittedBy", aggregate.SubmittedBy?.Value)
            .AddDateTime("SubmittedAtUtc", aggregate.SubmittedAtUtc?.Value)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_RootCauseAnalysis_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_RootCauseAnalysis_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"RootCauseAnalysis {aggregate.RootCauseAnalysisId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="RootCauseAnalysisAggregate"/> aggregate from its flat read-model row.</summary>
    private static RootCauseAnalysisAggregate Hydrate(RootCauseAnalysisRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<RootCauseAnalysisAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.RootCauseAnalysisId), RootCauseAnalysisId.FromGuid(row.RootCauseAnalysisId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.Status), Enum.Parse<RootCauseAnalysisStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.RelatedEventId), row.RelatedEventId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.Methodology), row.Methodology);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.LeadAnalyst), new UserId(row.LeadAnalyst));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.SubmittedBy), row.SubmittedBy.HasValue ? new UserId(row.SubmittedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.SubmittedAtUtc), row.SubmittedAtUtc.HasValue ? UtcTimestamp.From(row.SubmittedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.ApprovedBy), row.ApprovedBy.HasValue ? new UserId(row.ApprovedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RootCauseAnalysisAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IRootCauseAnalysisDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_RootCauseAnalysis_GetById</c>.</summary>
    private sealed record RootCauseAnalysisRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_RootCauseAnalysis_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer RootCauseAnalysisDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid RootCauseAnalysisId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public Guid? RelatedEventId { get; init; }
        public required string Methodology { get; init; }
        public required Guid LeadAnalyst { get; init; }
        public Guid? SubmittedBy { get; init; }
        public DateTime? SubmittedAtUtc { get; init; }
        public Guid? ApprovedBy { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
