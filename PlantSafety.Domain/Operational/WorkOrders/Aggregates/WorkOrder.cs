// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrder.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the WorkOrder bounded context -- the operational execution
//                  layer for maintenance work (create/assign/start/hold/resume/complete/cancel).
//                  New-build module: backs PlantSafety.UI/Modules/Maintenance's work-order core
//                  (v1/maintenance/work-orders*), which previously had no Application/API/Domain
//                  layer at all (see docs/Phase5/ui-gaps-scoping.md "Maintenance" section). Deliberately
//                  named "WorkOrders" (not "Maintenance") to avoid collision with the pre-existing,
//                  narrower Operational.PreventiveMaintenance and Operational.PredictiveMaintenance
//                  planning/scoring modules: WorkOrder is the operational execution layer; Preventive/
//                  Predictive are the planning/scoring layers that feed it (reused via MediatR query
//                  dispatch for the "preventive tasks" and "predictive insights" sub-resources rather
//                  than duplicated here). Mirrors
//                  PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates
//                  .PreventiveMaintenanceTask's guarded-transition/domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Enums;
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.Domain.Operational.WorkOrders.Exceptions;
using PlantSafety.Domain.Operational.WorkOrders.Rules;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Aggregates;

/// <summary>Aggregate root for the WorkOrder bounded context.</summary>
public sealed class WorkOrder
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this WorkOrder aggregate.</summary>
    public WorkOrderId WorkOrderId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public WorkOrderStatus Status { get; private set; }

    // ---- Work-order-specific business properties -------------------------------

    /// <summary>Human-readable work order code (format WO-###).</summary>
    public string WorkOrderCode { get; private set; } = string.Empty;

    /// <summary>Identifier of the machine/equipment this work order targets.</summary>
    public Guid MachineId { get; private set; }

    /// <summary>Human-readable machine code (format M-###), captured at creation for display.</summary>
    public string MachineCode { get; private set; } = string.Empty;

    /// <summary>Priority classification.</summary>
    public WorkOrderPriorityLevel Priority { get; private set; }

    /// <summary>Description of the work to be performed.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>UTC instant the work order is due.</summary>
    public UtcTimestamp DueAtUtc { get; private set; }

    // ---- Assignment ------------------------------------------------------------

    /// <summary>Identifier of the assigned technician, or null when unassigned.</summary>
    public Guid? TechnicianId { get; private set; }

    /// <summary>Display name of the assigned technician, or null when unassigned.</summary>
    public string? TechnicianName { get; private set; }

    /// <summary>UTC instant the technician was (last) assigned.</summary>
    public UtcTimestamp? AssignedAtUtc { get; private set; }

    // ---- Lifecycle attributes ---------------------------------------------------

    /// <summary>UTC instant the work order transitioned to InProgress (most recent).</summary>
    public UtcTimestamp? StartedAtUtc { get; private set; }

    /// <summary>UTC instant the work order was last placed on hold.</summary>
    public UtcTimestamp? OnHoldAtUtc { get; private set; }

    /// <summary>Identity of the user who completed the work order.</summary>
    public Guid? CompletedBy { get; private set; }

    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; private set; }

    /// <summary>UTC instant the work order was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Identity of the user who cancelled the work order.</summary>
    public Guid? CancelledBy { get; private set; }

    /// <summary>Reason recorded when a work order is cancelled.</summary>
    public string? CancelledReason { get; private set; }

    /// <summary>UTC instant the work order was cancelled.</summary>
    public UtcTimestamp? CancelledAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant of the most recent update to this record.</summary>
    public UtcTimestamp UpdatedAtUtc { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IWorkOrderDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWorkOrderDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWorkOrderDomainEvent>(_domainEvents);

    // For rehydration only.
    private WorkOrder() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new WorkOrder and records <see cref="WorkOrderCreatedEvent"/>.</summary>
    public static WorkOrder Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string workOrderCode,
        Guid machineId,
        string machineCode,
        WorkOrderPriorityLevel priority,
        string description,
        UtcTimestamp dueAtUtc)
    {
        if (string.IsNullOrWhiteSpace(workOrderCode))
            throw new ArgumentException("WorkOrderCode must be supplied.", nameof(workOrderCode));
        if (machineId == Guid.Empty)
            throw new ArgumentException("MachineId must be supplied.", nameof(machineId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description must be supplied.", nameof(description));

        var agg = new WorkOrder
        {
            WorkOrderId = WorkOrderId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = WorkOrderStatus.Open,
            WorkOrderCode = workOrderCode.Trim(),
            MachineId = machineId,
            MachineCode = (machineCode ?? string.Empty).Trim(),
            Priority = priority,
            Description = description.Trim(),
            DueAtUtc = dueAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = createdAtUtc,
        };

        agg.AddDomainEvent(new WorkOrderCreatedEvent(
            EventId: Guid.NewGuid(),
            WorkOrderId: agg.WorkOrderId,
            MachineId: machineId,
            WorkOrderCode: agg.WorkOrderCode,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Assignment ----------------------------------------------------------

    /// <summary>
    /// Assigns (or reassigns) a technician to this work order. Valid from
    /// <see cref="WorkOrderStatus.Open"/> or <see cref="WorkOrderStatus.Assigned"/>.
    /// </summary>
    public void Assign(
        Guid technicianId,
        string technicianName,
        UtcTimestamp assignedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!WorkOrderLifecycleRules.CanAssign(Status))
        {
            throw new WorkOrderInvalidStatusTransitionException(
                WorkOrderId, Status.ToString(), WorkOrderStatus.Assigned.ToString(), correlationId, causationId);
        }
        if (technicianId == Guid.Empty)
            throw new ArgumentException("TechnicianId must be supplied.", nameof(technicianId));

        TechnicianId = technicianId;
        TechnicianName = (technicianName ?? string.Empty).Trim();
        AssignedAtUtc = assignedAtUtc;
        Status = WorkOrderStatus.Assigned;
        UpdatedAtUtc = assignedAtUtc;

        AddDomainEvent(new WorkOrderAssignedEvent(
            EventId: Guid.NewGuid(),
            WorkOrderId: WorkOrderId,
            TechnicianId: technicianId,
            TechnicianName: TechnicianName,
            AssignedAtUtc: assignedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Generic status transitions (Start/PutOnHold/Resume/Cancel) -----------

    /// <summary>Starts work (Assigned -&gt; InProgress).</summary>
    public void Start(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => TransitionStatus(WorkOrderStatus.InProgress, WorkOrderLifecycleRules.CanStart, null, occurredAtUtc, actor, correlationId, causationId,
            () => StartedAtUtc = occurredAtUtc);

    /// <summary>Places the work order on hold (InProgress -&gt; OnHold).</summary>
    public void PutOnHold(string? reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => TransitionStatus(WorkOrderStatus.OnHold, WorkOrderLifecycleRules.CanPutOnHold, reason, occurredAtUtc, actor, correlationId, causationId,
            () => OnHoldAtUtc = occurredAtUtc);

    /// <summary>Resumes work from hold (OnHold -&gt; InProgress).</summary>
    public void Resume(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => TransitionStatus(WorkOrderStatus.InProgress, WorkOrderLifecycleRules.CanResume, null, occurredAtUtc, actor, correlationId, causationId,
            () => StartedAtUtc = occurredAtUtc);

    /// <summary>Cancels the work order before completion.</summary>
    public void Cancel(Guid cancelledBy, string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A cancellation reason must be supplied.", nameof(reason));

        TransitionStatus(WorkOrderStatus.Cancelled, WorkOrderLifecycleRules.CanCancel, reason, occurredAtUtc, actor, correlationId, causationId,
            () =>
            {
                CancelledBy = cancelledBy;
                CancelledReason = reason.Trim();
                CancelledAtUtc = occurredAtUtc;
            });
    }

    private void TransitionStatus(
        WorkOrderStatus target,
        Func<WorkOrderStatus, bool> guard,
        string? reason,
        UtcTimestamp occurredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        Action applySideEffects)
    {
        if (!guard(Status))
            throw new WorkOrderInvalidStatusTransitionException(WorkOrderId, Status.ToString(), target.ToString(), correlationId, causationId);

        var previous = Status;
        applySideEffects();
        Status = target;
        UpdatedAtUtc = occurredAtUtc;

        AddDomainEvent(new WorkOrderStatusChangedEvent(
            EventId: Guid.NewGuid(),
            WorkOrderId: WorkOrderId,
            PreviousStatus: previous,
            NewStatus: target,
            Reason: reason,
            ChangedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Completion --------------------------------------------------------

    /// <summary>Completes the work order (InProgress -&gt; Completed).</summary>
    public void Complete(
        Guid completedBy,
        string? completionNotes,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!WorkOrderLifecycleRules.CanComplete(Status))
        {
            throw new WorkOrderInvalidStatusTransitionException(
                WorkOrderId, Status.ToString(), WorkOrderStatus.Completed.ToString(), correlationId, causationId);
        }

        CompletedBy = completedBy;
        CompletionNotes = string.IsNullOrWhiteSpace(completionNotes) ? null : completionNotes.Trim();
        CompletedAtUtc = completedAtUtc;
        Status = WorkOrderStatus.Completed;
        UpdatedAtUtc = completedAtUtc;

        AddDomainEvent(new WorkOrderCompletedEvent(
            EventId: Guid.NewGuid(),
            WorkOrderId: WorkOrderId,
            CompletionNotes: CompletionNotes,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWorkOrderDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
