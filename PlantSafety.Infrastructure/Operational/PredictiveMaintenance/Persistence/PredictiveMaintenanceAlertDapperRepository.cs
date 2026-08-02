// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Infrastructure
//  File:           PredictiveMaintenanceAlertDapperRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPredictiveMaintenanceAlertRepository (Domain
//                  write-side repository interface for the PredictiveMaintenanceAlert aggregate
//                  root). All DB access via stored procedures in the [Operational] schema.
//                  Aggregate reconstruction uses AggregateReflectionHydrator because
//                  PredictiveMaintenanceAlert exposes only a private "for rehydration only"
//                  constructor and private-setter properties. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag from day one. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write
//                  -- a platform-wide bug fixed elsewhere in this program was exactly UpdateAsync
//                  forgetting to do this, so it is deliberately present here from day one.
//                  Distinct from PredictiveMaintenanceDapperRepository.cs (the pre-existing
//                  read-list implementation backing the Application-layer
//                  IPredictiveMaintenanceRepository/GetPredictiveMaintenanceQuery, which is left
//                  untouched by this pass). Mirrors PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Enums;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Events;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PredictiveMaintenanceAlert"/> aggregate root.</summary>
public sealed class PredictiveMaintenanceAlertDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PredictiveMaintenanceAlertDapperRepository> logger)
    : IPredictiveMaintenanceAlertRepository
{
    /// <inheritdoc/>
    public async Task<PredictiveMaintenanceAlert?> GetByIdAsync(PredictiveMaintenanceAlertId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveMaintenanceAlertId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PredictiveMaintenanceAlertRow>(
            "[Operational].[usp_PredictiveMaintenanceAlert_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PredictiveMaintenanceAlert aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId}", aggregate.PredictiveMaintenanceAlertId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveMaintenanceAlertId", aggregate.PredictiveMaintenanceAlertId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EquipmentId", aggregate.EquipmentId)
            .AddString("PredictedFailureType", aggregate.PredictedFailureType, 400)
            .AddDateTime("PredictedAtUtc", aggregate.PredictedAtUtc.Value)
            .AddGuid("ReviewedBy", aggregate.ReviewedBy?.Value)
            .AddDateTime("ReviewedAtUtc", aggregate.ReviewedAtUtc?.Value)
            .AddGuid("ActionScheduledBy", aggregate.ActionScheduledBy?.Value)
            .AddString("WorkOrderReference", aggregate.WorkOrderReference, 200)
            .AddDateTime("ActionScheduledAtUtc", aggregate.ActionScheduledAtUtc?.Value)
            .AddGuid("ResolvedBy", aggregate.ResolvedBy?.Value)
            .AddString("ResolutionNotes", aggregate.ResolutionNotes, 2000)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddGuid("DismissedBy", aggregate.DismissedBy?.Value)
            .AddString("DismissedReason", aggregate.DismissedReason, 2000)
            .AddDateTime("DismissedAtUtc", aggregate.DismissedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_PredictiveMaintenanceAlert_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised by
        // the aggregate during this write so TransactionalUnitOfWork.CommitAsync (invoked by
        // TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PredictiveMaintenanceAlert aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId}", aggregate.PredictiveMaintenanceAlertId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveMaintenanceAlertId", aggregate.PredictiveMaintenanceAlertId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EquipmentId", aggregate.EquipmentId)
            .AddString("PredictedFailureType", aggregate.PredictedFailureType, 400)
            .AddDateTime("PredictedAtUtc", aggregate.PredictedAtUtc.Value)
            .AddGuid("ReviewedBy", aggregate.ReviewedBy?.Value)
            .AddDateTime("ReviewedAtUtc", aggregate.ReviewedAtUtc?.Value)
            .AddGuid("ActionScheduledBy", aggregate.ActionScheduledBy?.Value)
            .AddString("WorkOrderReference", aggregate.WorkOrderReference, 200)
            .AddDateTime("ActionScheduledAtUtc", aggregate.ActionScheduledAtUtc?.Value)
            .AddGuid("ResolvedBy", aggregate.ResolvedBy?.Value)
            .AddString("ResolutionNotes", aggregate.ResolutionNotes, 2000)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddGuid("DismissedBy", aggregate.DismissedBy?.Value)
            .AddString("DismissedReason", aggregate.DismissedReason, 2000)
            .AddDateTime("DismissedAtUtc", aggregate.DismissedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PredictiveMaintenanceAlert_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Operational].[usp_PredictiveMaintenanceAlert_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PredictiveMaintenanceAlert {aggregate.PredictiveMaintenanceAlertId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition (Review, ScheduleAction, Resolve, Dismiss,
        // Close) would silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PredictiveMaintenanceAlert"/> aggregate from its flat read-model row.</summary>
    private static PredictiveMaintenanceAlert Hydrate(PredictiveMaintenanceAlertRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PredictiveMaintenanceAlert>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.PredictiveMaintenanceAlertId), PredictiveMaintenanceAlertId.FromGuid(row.PredictiveMaintenanceAlertId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.Status), Enum.Parse<PredictiveMaintenanceAlertStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.EquipmentId), row.EquipmentId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.PredictedFailureType), row.PredictedFailureType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.PredictedAtUtc), UtcTimestamp.From(row.PredictedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ReviewedBy), row.ReviewedBy.HasValue ? new UserId(row.ReviewedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ReviewedAtUtc), row.ReviewedAtUtc.HasValue ? UtcTimestamp.From(row.ReviewedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ActionScheduledBy), row.ActionScheduledBy.HasValue ? new UserId(row.ActionScheduledBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.WorkOrderReference), row.WorkOrderReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ActionScheduledAtUtc), row.ActionScheduledAtUtc.HasValue ? UtcTimestamp.From(row.ActionScheduledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ResolvedBy), row.ResolvedBy.HasValue ? new UserId(row.ResolvedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ResolutionNotes), row.ResolutionNotes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ResolvedAtUtc), row.ResolvedAtUtc.HasValue ? UtcTimestamp.From(row.ResolvedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.DismissedBy), row.DismissedBy.HasValue ? new UserId(row.DismissedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.DismissedReason), row.DismissedReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.DismissedAtUtc), row.DismissedAtUtc.HasValue ? UtcTimestamp.From(row.DismissedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PredictiveMaintenanceAlert.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPredictiveMaintenanceAlertDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PredictiveMaintenanceAlert_GetById</c>.</summary>
    private sealed record PredictiveMaintenanceAlertRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PredictiveMaintenanceAlert_GetById</c>. Not consumed by aggregate hydration
        /// -- callers obtain the ETag for a subsequent write from a prior single-record read (see
        /// the Application-layer PredictiveMaintenanceAlertItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PredictiveMaintenanceAlertId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid EquipmentId { get; init; }
        public required string PredictedFailureType { get; init; }
        public required DateTime PredictedAtUtc { get; init; }
        public Guid? ReviewedBy { get; init; }
        public DateTime? ReviewedAtUtc { get; init; }
        public Guid? ActionScheduledBy { get; init; }
        public string? WorkOrderReference { get; init; }
        public DateTime? ActionScheduledAtUtc { get; init; }
        public Guid? ResolvedBy { get; init; }
        public string? ResolutionNotes { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public Guid? DismissedBy { get; init; }
        public string? DismissedReason { get; init; }
        public DateTime? DismissedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
