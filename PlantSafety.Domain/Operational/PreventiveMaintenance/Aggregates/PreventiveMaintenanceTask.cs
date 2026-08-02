// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceTask.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PreventiveMaintenanceTask bounded context. Tracks a
//                  single scheduled maintenance task from scheduling through work start,
//                  completion, independent verification, cancellation, or overdue detection.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at
//                  all -- only a placeholder marker class (PreventiveMaintenanceModuleMarker) and
//                  a read-only query slice over a "PreventiveMaintenanceReadModel" table existed.
//                  Field shape (EquipmentId, TaskDescription, Status, ScheduledAtUtc, CompletedBy)
//                  is grounded in that pre-existing read model's actual columns, not invented from
//                  scratch. Mirrors PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates
//                  .PredictiveMaintenanceAlert's guarded-transition/domain-event/exception
//                  conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.PreventiveMaintenance.Enums;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Events;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Exceptions;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Rules;
using PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates;

/// <summary>Aggregate root for the PreventiveMaintenanceTask bounded context.</summary>
public sealed class PreventiveMaintenanceTask
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this PreventiveMaintenanceTask aggregate.</summary>
    public PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PreventiveMaintenanceTaskStatus Status { get; private set; }

    // ---- Task-specific business properties -------------------------------------

    /// <summary>Identifier of the equipment scheduled for maintenance.</summary>
    public Guid EquipmentId { get; private set; }

    /// <summary>Description of the maintenance task.</summary>
    public string TaskDescription { get; private set; } = string.Empty;

    /// <summary>UTC instant the task is/was scheduled for.</summary>
    public UtcTimestamp ScheduledAtUtc { get; private set; }

    // ---- Lifecycle attributes (Start/Complete/Verify/Cancel/MarkOverdue) -------

    /// <summary>Identity of the user who started the maintenance work.</summary>
    public UserId? StartedBy { get; private set; }

    /// <summary>UTC instant maintenance work began.</summary>
    public UtcTimestamp? StartedAtUtc { get; private set; }

    /// <summary>Identity of the user who completed the maintenance work.</summary>
    public UserId? CompletedBy { get; private set; }

    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; private set; }

    /// <summary>UTC instant maintenance work was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Identity of the user who verified the completed work.</summary>
    public UserId? VerifiedBy { get; private set; }

    /// <summary>UTC instant the completed work was verified.</summary>
    public UtcTimestamp? VerifiedAtUtc { get; private set; }

    /// <summary>Identity of the user who cancelled the task.</summary>
    public UserId? CancelledBy { get; private set; }

    /// <summary>Reason recorded when a task is cancelled.</summary>
    public string? CancelledReason { get; private set; }

    /// <summary>UTC instant the task was cancelled.</summary>
    public UtcTimestamp? CancelledAtUtc { get; private set; }

    /// <summary>UTC instant the task was marked overdue.</summary>
    public UtcTimestamp? OverdueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IPreventiveMaintenanceTaskDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPreventiveMaintenanceTaskDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPreventiveMaintenanceTaskDomainEvent>(_domainEvents);

    // For rehydration only.
    private PreventiveMaintenanceTask() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Schedules a new PreventiveMaintenanceTask and records <see cref="PreventiveMaintenanceTaskScheduledEvent"/>.</summary>
    public static PreventiveMaintenanceTask Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid equipmentId,
        string taskDescription,
        UtcTimestamp scheduledAtUtc)
    {
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("EquipmentId must be supplied.", nameof(equipmentId));
        if (string.IsNullOrWhiteSpace(taskDescription))
            throw new ArgumentException("TaskDescription must be supplied.", nameof(taskDescription));

        var agg = new PreventiveMaintenanceTask
        {
            PreventiveMaintenanceTaskId = PreventiveMaintenanceTaskId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = PreventiveMaintenanceTaskStatus.Scheduled,
            EquipmentId = equipmentId,
            TaskDescription = taskDescription.Trim(),
            ScheduledAtUtc = scheduledAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new PreventiveMaintenanceTaskScheduledEvent(
            EventId: PreventiveMaintenanceEventId.New(),
            PreventiveMaintenanceTaskId: agg.PreventiveMaintenanceTaskId,
            EquipmentId: equipmentId,
            TaskDescription: agg.TaskDescription,
            ScheduledAtUtc: scheduledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: Start ----------------------------------------------------

    /// <summary>
    /// Records that maintenance work has begun (Scheduled -&gt; InProgress). Valid only from
    /// <see cref="PreventiveMaintenanceTaskStatus.Scheduled"/>.
    /// </summary>
    public void Start(
        UserId startedBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PreventiveMaintenanceTaskLifecycleRules.CanStart, PreventiveMaintenanceTaskStatus.InProgress);

        StartedBy = startedBy;
        StartedAtUtc = startedAtUtc;
        Status = PreventiveMaintenanceTaskStatus.InProgress;

        AddDomainEvent(new PreventiveMaintenanceTaskStartedEvent(
            EventId: PreventiveMaintenanceEventId.New(),
            PreventiveMaintenanceTaskId: PreventiveMaintenanceTaskId,
            StartedBy: startedBy,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete / Verify ----------------------------------------

    /// <summary>
    /// Records that maintenance work has been completed (InProgress -&gt; Completed). Valid only
    /// from <see cref="PreventiveMaintenanceTaskStatus.InProgress"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        string? completionNotes,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PreventiveMaintenanceTaskLifecycleRules.CanComplete, PreventiveMaintenanceTaskStatus.Completed);

        CompletedBy = completedBy;
        CompletionNotes = string.IsNullOrWhiteSpace(completionNotes) ? null : completionNotes.Trim();
        CompletedAtUtc = completedAtUtc;
        Status = PreventiveMaintenanceTaskStatus.Completed;

        AddDomainEvent(new PreventiveMaintenanceTaskCompletedEvent(
            EventId: PreventiveMaintenanceEventId.New(),
            PreventiveMaintenanceTaskId: PreventiveMaintenanceTaskId,
            CompletedBy: completedBy,
            CompletionNotes: CompletionNotes,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records independent verification of completed work (Completed -&gt; Verified). Valid only
    /// from <see cref="PreventiveMaintenanceTaskStatus.Completed"/>.
    /// </summary>
    public void Verify(
        UserId verifiedBy,
        UtcTimestamp verifiedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PreventiveMaintenanceTaskLifecycleRules.CanVerify, PreventiveMaintenanceTaskStatus.Verified);

        VerifiedBy = verifiedBy;
        VerifiedAtUtc = verifiedAtUtc;
        Status = PreventiveMaintenanceTaskStatus.Verified;

        AddDomainEvent(new PreventiveMaintenanceTaskVerifiedEvent(
            EventId: PreventiveMaintenanceEventId.New(),
            PreventiveMaintenanceTaskId: PreventiveMaintenanceTaskId,
            VerifiedBy: verifiedBy,
            VerifiedAtUtc: verifiedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Cancel / MarkOverdue -------------------------------------

    /// <summary>
    /// Cancels the task before completion. Valid from <see cref="PreventiveMaintenanceTaskStatus.Scheduled"/>
    /// or <see cref="PreventiveMaintenanceTaskStatus.InProgress"/>.
    /// </summary>
    public void Cancel(
        UserId cancelledBy,
        string reason,
        UtcTimestamp cancelledAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PreventiveMaintenanceTaskLifecycleRules.CanCancel, PreventiveMaintenanceTaskStatus.Cancelled);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A cancellation reason must be supplied.", nameof(reason));

        CancelledBy = cancelledBy;
        CancelledReason = reason.Trim();
        CancelledAtUtc = cancelledAtUtc;
        Status = PreventiveMaintenanceTaskStatus.Cancelled;

        AddDomainEvent(new PreventiveMaintenanceTaskCancelledEvent(
            EventId: PreventiveMaintenanceEventId.New(),
            PreventiveMaintenanceTaskId: PreventiveMaintenanceTaskId,
            CancelledBy: cancelledBy,
            Reason: CancelledReason,
            CancelledAtUtc: cancelledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Marks a scheduled task overdue after it passes its due date without starting/completing.
    /// Valid only from <see cref="PreventiveMaintenanceTaskStatus.Scheduled"/>.
    /// </summary>
    public void MarkOverdue(
        UtcTimestamp overdueAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PreventiveMaintenanceTaskLifecycleRules.CanMarkOverdue, PreventiveMaintenanceTaskStatus.Overdue);

        OverdueAtUtc = overdueAtUtc;
        Status = PreventiveMaintenanceTaskStatus.Overdue;

        AddDomainEvent(new PreventiveMaintenanceTaskOverdueEvent(
            EventId: PreventiveMaintenanceEventId.New(),
            PreventiveMaintenanceTaskId: PreventiveMaintenanceTaskId,
            OverdueAtUtc: overdueAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPreventiveMaintenanceTaskDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<PreventiveMaintenanceTaskStatus, bool> guard, PreventiveMaintenanceTaskStatus expected)
    {
        if (guard(Status)) return;

        throw new PreventiveMaintenanceTaskInvalidStatusTransitionException(
            PreventiveMaintenanceTaskId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
