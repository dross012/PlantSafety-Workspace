// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Infrastructure
//  File:           WorkOrderDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IWorkOrderRepository (Domain repository
//                  interface for the WorkOrder aggregate root). All DB access via stored procedures
//                  in the [Operational] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because WorkOrder exposes only a private
//                  "for rehydration only" constructor and private-setter properties. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write,
//                  AND additionally persist one [Operational].[MaintenanceEvents] row per domain
//                  event so GetWorkOrderHistoryQuery/GetMachineHistoryQuery are backed by genuine
//                  data rather than an empty placeholder table. Mirrors
//                  PlantSafety.Infrastructure.Operational.PreventiveMaintenance.Persistence
//                  .PreventiveMaintenanceTaskDapperRepository. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Operational.WorkOrders.Aggregates;
using PlantSafety.Domain.Operational.WorkOrders.Enums;
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.Domain.Operational.WorkOrders.Interfaces;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Operational.WorkOrders.Persistence;

/// <summary>Dapper-backed repository for the <see cref="WorkOrder"/> aggregate root.</summary>
public sealed class WorkOrderDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WorkOrderDapperRepository> logger)
    : IWorkOrderRepository
{
    /// <inheritdoc/>
    public async Task<WorkOrder?> GetByIdAsync(WorkOrderId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for WorkOrder {WorkOrderId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkOrderId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkOrderRow>(
            "[Operational].[usp_WorkOrder_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(WorkOrder aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add WorkOrder {WorkOrderId}", aggregate.WorkOrderId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkOrderId", aggregate.WorkOrderId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("WorkOrderCode", aggregate.WorkOrderCode, 50)
            .AddGuid("MachineId", aggregate.MachineId)
            .AddString("MachineCode", aggregate.MachineCode, 50)
            .AddString("Priority", aggregate.Priority.ToString(), 20)
            .AddString("Description", aggregate.Description, 2000)
            .AddDateTime("DueAtUtc", aggregate.DueAtUtc.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_WorkOrder_Insert]", parameters, tx, cancellationToken: cancellationToken);

        await PersistDomainEventsAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(WorkOrder aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update WorkOrder {WorkOrderId}", aggregate.WorkOrderId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkOrderId", aggregate.WorkOrderId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("TechnicianId", aggregate.TechnicianId)
            .AddString("TechnicianName", aggregate.TechnicianName, 200)
            .AddDateTime("AssignedAtUtc", aggregate.AssignedAtUtc?.Value)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc?.Value)
            .AddDateTime("OnHoldAtUtc", aggregate.OnHoldAtUtc?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy)
            .AddString("CompletionNotes", aggregate.CompletionNotes, 2000)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("CancelledBy", aggregate.CancelledBy)
            .AddString("CancelledReason", aggregate.CancelledReason, 2000)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddDateTime("UpdatedAtUtc", aggregate.UpdatedAtUtc.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Operational].[usp_WorkOrder_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"WorkOrder {aggregate.WorkOrderId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        await PersistDomainEventsAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>
    /// Persists one [Operational].[MaintenanceEvents] row per domain event raised by
    /// <paramref name="aggregate"/> during this unit of work, within the same transaction as the
    /// aggregate write.
    /// </summary>
    private async Task PersistDomainEventsAsync(IDbConnection conn, IDbTransaction tx, WorkOrder aggregate, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            var summary = DescribeEvent(domainEvent);
            var eventParameters = new StoredProcedureBuilder()
                .AddGuid("EventId", Guid.NewGuid())
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("WorkOrderId", aggregate.WorkOrderId.Value)
                .AddString("WorkOrderCode", aggregate.WorkOrderCode, 50)
                .AddGuid("MachineId", aggregate.MachineId)
                .AddDateTime("OccurredAtUtc", domainEvent.OccurredAtUtc.Value)
                .AddString("TechnicianName", aggregate.TechnicianName, 200)
                .AddString("Summary", summary, 500)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_MaintenanceEvent_Insert]", eventParameters, tx, cancellationToken: cancellationToken);
        }
    }

    private static string DescribeEvent(IWorkOrderDomainEvent domainEvent) => domainEvent switch
    {
        WorkOrderCreatedEvent e => $"Work order {e.WorkOrderCode} created.",
        WorkOrderAssignedEvent e => $"Assigned to {e.TechnicianName}.",
        WorkOrderStatusChangedEvent e => $"Status changed from {e.PreviousStatus} to {e.NewStatus}" + (string.IsNullOrWhiteSpace(e.Reason) ? "." : $" ({e.Reason})."),
        WorkOrderCompletedEvent => "Work order completed.",
        _ => domainEvent.EventType,
    };

    /// <summary>Reconstructs a <see cref="WorkOrder"/> aggregate from its flat read-model row.</summary>
    private static WorkOrder Hydrate(WorkOrderRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<WorkOrder>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.WorkOrderId), WorkOrderId.FromGuid(row.WorkOrderId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.Status), Enum.Parse<WorkOrderStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.WorkOrderCode), row.WorkOrderCode);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.MachineId), row.MachineId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.MachineCode), row.MachineCode);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.Priority), Enum.Parse<WorkOrderPriorityLevel>(row.Priority));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.DueAtUtc), UtcTimestamp.From(row.DueAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.TechnicianId), row.TechnicianId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.TechnicianName), row.TechnicianName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.AssignedAtUtc), row.AssignedAtUtc.HasValue ? UtcTimestamp.From(row.AssignedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.StartedAtUtc), row.StartedAtUtc.HasValue ? UtcTimestamp.From(row.StartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.OnHoldAtUtc), row.OnHoldAtUtc.HasValue ? UtcTimestamp.From(row.OnHoldAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CompletedBy), row.CompletedBy);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CompletionNotes), row.CompletionNotes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CancelledBy), row.CancelledBy);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CancelledReason), row.CancelledReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CancelledAtUtc), row.CancelledAtUtc.HasValue ? UtcTimestamp.From(row.CancelledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(WorkOrder.UpdatedAtUtc), UtcTimestamp.From(row.UpdatedAtUtc ?? row.CreatedAtUtc));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IWorkOrderDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_WorkOrder_GetById</c>.</summary>
    private sealed record WorkOrderRow
    {
        public string? ETag { get; init; }

        public required Guid WorkOrderId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string WorkOrderCode { get; init; }
        public required Guid MachineId { get; init; }
        public required string MachineCode { get; init; }
        public required string Priority { get; init; }
        public required string Description { get; init; }
        public required DateTime DueAtUtc { get; init; }
        public Guid? TechnicianId { get; init; }
        public string? TechnicianName { get; init; }
        public DateTime? AssignedAtUtc { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        public DateTime? OnHoldAtUtc { get; init; }
        public Guid? CompletedBy { get; init; }
        public string? CompletionNotes { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public Guid? CancelledBy { get; init; }
        public string? CancelledReason { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
