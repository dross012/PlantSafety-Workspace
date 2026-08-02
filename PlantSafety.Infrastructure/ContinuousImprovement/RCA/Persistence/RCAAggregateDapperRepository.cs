// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Infrastructure
//  File:           RCAAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer write-side
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces.IRCARepository for the
//                  RCA aggregate root. All DB access via stored procedures in the
//                  [ContinuousImprovement] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because RCA exposes only a private "for rehydration
//                  only" constructor and private-setter properties. GetByIdAsync is tenant-scoped
//                  and UpdateAsync enforces optimistic concurrency via @ETag from day one. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write.
//                  Named "RCAAggregateDapperRepository" (rather than "RCADapperRepository") to
//                  avoid colliding with the pre-existing read-list RCADapperRepository already
//                  declared in the sibling Persistence namespace (backs the unrelated, untouched
//                  date-range GetRCAQuery/usp_RCA_GetRcaRecords read path). Mirrors
//                  PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence
//                  .RootCauseAnalysisAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.ContinuousImprovement.RCA.Enums;
using PlantSafety.Domain.ContinuousImprovement.RCA.Events;
using PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using RCAAggregate = PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA;

namespace PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence;

/// <summary>Dapper-backed repository for the <see cref="RCAAggregate"/> aggregate root.</summary>
public sealed class RCAAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RCAAggregateDapperRepository> logger)
    : IRCARepository
{
    /// <inheritdoc/>
    public async Task<RCAAggregate?> GetByIdAsync(RCAId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RCA {RcaId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RcaId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<RCARow>(
            "[ContinuousImprovement].[usp_RCA_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(RCAAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add RCA {RcaId}", aggregate.RcaId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RcaId", aggregate.RcaId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("ProblemStatement", aggregate.ProblemStatement, size: -1)
            .AddString("Methodology", aggregate.Methodology, 50)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("InitiatedAtUtc", aggregate.InitiatedAtUtc.Value)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddGuid("StartedBy", aggregate.StartedBy?.Value)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc?.Value)
            .AddGuid("SubmittedBy", aggregate.SubmittedBy?.Value)
            .AddString("Findings", aggregate.Findings, size: -1)
            .AddDateTime("SubmittedAtUtc", aggregate.SubmittedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddString("RootCauseSummary", aggregate.RootCauseSummary, size: -1)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ContinuousImprovement].[usp_RCA_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(RCAAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update RCA {RcaId}", aggregate.RcaId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RcaId", aggregate.RcaId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("ProblemStatement", aggregate.ProblemStatement, size: -1)
            .AddString("Methodology", aggregate.Methodology, 50)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("StartedBy", aggregate.StartedBy?.Value)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc?.Value)
            .AddGuid("SubmittedBy", aggregate.SubmittedBy?.Value)
            .AddString("Findings", aggregate.Findings, size: -1)
            .AddDateTime("SubmittedAtUtc", aggregate.SubmittedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddString("RootCauseSummary", aggregate.RootCauseSummary, size: -1)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("ModifiedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_RCA_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[ContinuousImprovement].[usp_RCA_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"RCA {aggregate.RcaId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="RCAAggregate"/> aggregate from its flat read-model row.</summary>
    private static RCAAggregate Hydrate(RCARow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<RCAAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.RcaId), RCAId.FromGuid(row.RcaId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.Status), Enum.Parse<RCAStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.ProblemStatement), row.ProblemStatement);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.Methodology), row.Methodology);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.InitiatedAtUtc), UtcTimestamp.From(row.InitiatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.OwnerUserId), new UserId(row.OwnerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.StartedBy), row.StartedBy.HasValue ? new UserId(row.StartedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.StartedAtUtc), row.StartedAtUtc.HasValue ? UtcTimestamp.From(row.StartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.SubmittedBy), row.SubmittedBy.HasValue ? new UserId(row.SubmittedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.Findings), row.Findings);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.SubmittedAtUtc), row.SubmittedAtUtc.HasValue ? UtcTimestamp.From(row.SubmittedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.RootCauseSummary), row.RootCauseSummary);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RCAAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IRCADomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_RCA_GetById</c>.</summary>
    private sealed record RCARow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_RCA_GetById</c>. Not consumed by aggregate hydration -- callers obtain the
        /// ETag for a subsequent write from a prior single-record read (see the Application-layer
        /// RCADetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid RcaId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ProblemStatement { get; init; }
        public required string Methodology { get; init; }
        public required string Status { get; init; }
        public required DateTime InitiatedAtUtc { get; init; }
        public required Guid OwnerUserId { get; init; }
        public Guid? StartedBy { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        public Guid? SubmittedBy { get; init; }
        public string? Findings { get; init; }
        public DateTime? SubmittedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public string? RootCauseSummary { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
