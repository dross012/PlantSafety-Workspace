// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Infrastructure
//  File:           PreventiveMaintenanceTaskDapperRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPreventiveMaintenanceTaskRepository (Domain
//                  repository interface for the PreventiveMaintenanceTask aggregate root). All DB
//                  access via stored procedures in the [Operational] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because PreventiveMaintenanceTask
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. Both AddAsync AND UpdateAsync enqueue domain events to the outbox
//                  after their write -- a platform-wide bug fixed elsewhere in this program was
//                  exactly UpdateAsync forgetting to do this, so it is deliberately present here
//                  from day one. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag from day one (this aggregate had no prior write path to
//                  retrofit). Distinct from the pre-existing PreventiveMaintenanceDapperRepository
//                  in this same module (which implements the Application-layer read-only
//                  IPreventiveMaintenanceRepository date-range list query and is left untouched).
//                  Mirrors PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Enums;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Events;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Operational.PreventiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PreventiveMaintenanceTask"/> aggregate root.</summary>
public sealed class PreventiveMaintenanceTaskDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PreventiveMaintenanceTaskDapperRepository> logger)
    : IPreventiveMaintenanceTaskRepository
{
    /// <inheritdoc/>
    public async Task<PreventiveMaintenanceTask?> GetByIdAsync(PreventiveMaintenanceTaskId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PreventiveMaintenanceTask {PreventiveMaintenanceTaskId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PreventiveMaintenanceTaskId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PreventiveMaintenanceTaskRow>(
            "[Operational].[usp_PreventiveMaintenanceTask_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PreventiveMaintenanceTask aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PreventiveMaintenanceTask {PreventiveMaintenanceTaskId}", aggregate.PreventiveMaintenanceTaskId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PreventiveMaintenanceTaskId", aggregate.PreventiveMaintenanceTaskId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EquipmentId", aggregate.EquipmentId)
            .AddString("TaskDescription", aggregate.TaskDescription, 400)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc.Value)
            .AddGuid("StartedBy", aggregate.StartedBy?.Value)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddString("CompletionNotes", aggregate.CompletionNotes, 2000)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("VerifiedBy", aggregate.VerifiedBy?.Value)
            .AddDateTime("VerifiedAtUtc", aggregate.VerifiedAtUtc?.Value)
            .AddGuid("CancelledBy", aggregate.CancelledBy?.Value)
            .AddString("CancelledReason", aggregate.CancelledReason, 2000)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddDateTime("OverdueAtUtc", aggregate.OverdueAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_PreventiveMaintenanceTask_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised by the
        // aggregate during this write so TransactionalUnitOfWork.CommitAsync (invoked by
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
    public async Task UpdateAsync(PreventiveMaintenanceTask aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PreventiveMaintenanceTask {PreventiveMaintenanceTaskId}", aggregate.PreventiveMaintenanceTaskId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PreventiveMaintenanceTaskId", aggregate.PreventiveMaintenanceTaskId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EquipmentId", aggregate.EquipmentId)
            .AddString("TaskDescription", aggregate.TaskDescription, 400)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc.Value)
            .AddGuid("StartedBy", aggregate.StartedBy?.Value)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddString("CompletionNotes", aggregate.CompletionNotes, 2000)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("VerifiedBy", aggregate.VerifiedBy?.Value)
            .AddDateTime("VerifiedAtUtc", aggregate.VerifiedAtUtc?.Value)
            .AddGuid("CancelledBy", aggregate.CancelledBy?.Value)
            .AddString("CancelledReason", aggregate.CancelledReason, 2000)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddDateTime("OverdueAtUtc", aggregate.OverdueAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PreventiveMaintenanceTask_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Operational].[usp_PreventiveMaintenanceTask_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PreventiveMaintenanceTask {aggregate.PreventiveMaintenanceTaskId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised during
        // this lifecycle transition. UpdateAsync must do this too, not just AddAsync -- otherwise
        // every post-scheduling transition (Start, Complete, Verify, Cancel, MarkOverdue) would
        // silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PreventiveMaintenanceTask"/> aggregate from its flat read-model row.</summary>
    private static PreventiveMaintenanceTask Hydrate(PreventiveMaintenanceTaskRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PreventiveMaintenanceTask>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.PreventiveMaintenanceTaskId), PreventiveMaintenanceTaskId.FromGuid(row.PreventiveMaintenanceTaskId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.Status), Enum.Parse<PreventiveMaintenanceTaskStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.EquipmentId), row.EquipmentId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.TaskDescription), row.TaskDescription);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.ScheduledAtUtc), UtcTimestamp.From(row.ScheduledAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.StartedBy), row.StartedBy.HasValue ? new UserId(row.StartedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.StartedAtUtc), row.StartedAtUtc.HasValue ? UtcTimestamp.From(row.StartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CompletedBy), row.CompletedBy.HasValue ? new UserId(row.CompletedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CompletionNotes), row.CompletionNotes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.VerifiedBy), row.VerifiedBy.HasValue ? new UserId(row.VerifiedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.VerifiedAtUtc), row.VerifiedAtUtc.HasValue ? UtcTimestamp.From(row.VerifiedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CancelledBy), row.CancelledBy.HasValue ? new UserId(row.CancelledBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CancelledReason), row.CancelledReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CancelledAtUtc), row.CancelledAtUtc.HasValue ? UtcTimestamp.From(row.CancelledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.OverdueAtUtc), row.OverdueAtUtc.HasValue ? UtcTimestamp.From(row.OverdueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PreventiveMaintenanceTask.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPreventiveMaintenanceTaskDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PreventiveMaintenanceTask_GetById</c>.</summary>
    private sealed record PreventiveMaintenanceTaskRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PreventiveMaintenanceTask_GetById</c>. Not consumed by aggregate hydration --
        /// callers obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer PreventiveMaintenanceTaskItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PreventiveMaintenanceTaskId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid EquipmentId { get; init; }
        public required string TaskDescription { get; init; }
        public required DateTime ScheduledAtUtc { get; init; }
        public Guid? StartedBy { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        public Guid? CompletedBy { get; init; }
        public string? CompletionNotes { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public Guid? VerifiedBy { get; init; }
        public DateTime? VerifiedAtUtc { get; init; }
        public Guid? CancelledBy { get; init; }
        public string? CancelledReason { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public DateTime? OverdueAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
