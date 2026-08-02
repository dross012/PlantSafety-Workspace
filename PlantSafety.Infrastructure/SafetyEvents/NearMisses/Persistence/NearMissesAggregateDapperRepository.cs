// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Infrastructure
//  File:           NearMissesAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of INearMissesRepository (Domain write-side
//                  repository interface for the NearMisses aggregate root). All DB access via
//                  stored procedures in the [SafetyEvents] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because NearMisses exposes only a private "for
//                  rehydration only" constructor and private-setter properties. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from day
//                  one. Both AddAsync AND UpdateAsync enqueue domain events to the outbox after
//                  their write. Named "...AggregateDapperRepository" (rather than
//                  "NearMissesDapperRepository") to avoid colliding with the pre-existing
//                  read-list NearMissesDapperRepository already declared in this same
//                  Persistence namespace. The aggregate type itself is imported via the
//                  "NearMissesAggregate" alias rather than its bare name "NearMisses" -- an
//                  unqualified "NearMisses" inside this file's own
//                  PlantSafety.Infrastructure.SafetyEvents.NearMisses.Persistence namespace binds
//                  to the enclosing PlantSafety.Infrastructure.SafetyEvents.NearMisses namespace
//                  segment before any using-alias is consulted, which a same-named using-alias
//                  cannot override. Mirrors
//                  PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence
//                  .PredictiveMaintenanceAlertDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.NearMisses.Enums;
using PlantSafety.Domain.SafetyEvents.NearMisses.Events;
using PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces;
using PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using NearMissesAggregate = PlantSafety.Domain.SafetyEvents.NearMisses.Aggregates.NearMisses;

namespace PlantSafety.Infrastructure.SafetyEvents.NearMisses.Persistence;

/// <summary>Dapper-backed repository for the <see cref="NearMissesAggregate"/> aggregate root.</summary>
public sealed class NearMissesAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<NearMissesAggregateDapperRepository> logger)
    : INearMissesRepository
{
    /// <inheritdoc/>
    public async Task<NearMissesAggregate?> GetByIdAsync(NearMissesId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for NearMisses {NearMissesId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("NearMissesId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<NearMissesRow>(
            "[SafetyEvents].[usp_NearMisses_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(NearMissesAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add NearMisses {NearMissesId}", aggregate.NearMissesId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("NearMissesId", aggregate.NearMissesId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Category", aggregate.Category, 100)
            .AddString("PotentialSeverity", aggregate.PotentialSeverity, 50)
            .AddDateTime("ReportedAtUtc", aggregate.ReportedAtUtc.Value)
            .AddGuid("ReportedBy", aggregate.CreatedBy.Value)
            .AddGuid("ReviewStartedBy", aggregate.ReviewStartedBy?.Value)
            .AddDateTime("ReviewStartedAtUtc", aggregate.ReviewStartedAtUtc?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddString("Findings", aggregate.Findings, 2000)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_NearMisses_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(NearMissesAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update NearMisses {NearMissesId}", aggregate.NearMissesId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("NearMissesId", aggregate.NearMissesId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Category", aggregate.Category, 100)
            .AddString("PotentialSeverity", aggregate.PotentialSeverity, 50)
            .AddDateTime("ReportedAtUtc", aggregate.ReportedAtUtc.Value)
            .AddGuid("ReviewStartedBy", aggregate.ReviewStartedBy?.Value)
            .AddDateTime("ReviewStartedAtUtc", aggregate.ReviewStartedAtUtc?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddString("Findings", aggregate.Findings, 2000)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_NearMisses_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_NearMisses_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"NearMisses {aggregate.NearMissesId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="NearMissesAggregate"/> aggregate from its flat read-model row.</summary>
    private static NearMissesAggregate Hydrate(NearMissesRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<NearMissesAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.NearMissesId), NearMissesId.FromGuid(row.NearMissesId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.Status), Enum.Parse<NearMissesStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.Category), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.PotentialSeverity), row.PotentialSeverity);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.ReportedAtUtc), UtcTimestamp.From(row.ReportedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.ReviewStartedBy), row.ReviewStartedBy.HasValue ? new UserId(row.ReviewStartedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.ReviewStartedAtUtc), row.ReviewStartedAtUtc.HasValue ? UtcTimestamp.From(row.ReviewStartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.CompletedBy), row.CompletedBy.HasValue ? new UserId(row.CompletedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.Findings), row.Findings);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(NearMissesAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<INearMissesDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_NearMisses_GetById</c>.</summary>
    private sealed record NearMissesRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_NearMisses_GetById</c>. Not consumed by aggregate hydration -- callers obtain
        /// the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer NearMissesDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid NearMissesId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string Category { get; init; }
        public required string PotentialSeverity { get; init; }
        public required DateTime ReportedAtUtc { get; init; }
        public Guid? ReportedBy { get; init; }
        public Guid? ReviewStartedBy { get; init; }
        public DateTime? ReviewStartedAtUtc { get; init; }
        public Guid? CompletedBy { get; init; }
        public string? Findings { get; init; }
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
