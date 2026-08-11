// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMScheduling.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PMScheduling bounded context (module: Scheduling).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Aggregates;

/// <summary>Aggregate root for the PMScheduling bounded context.</summary>
public sealed partial class PMScheduling
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PMScheduling aggregate.</summary>
    public PMSchedulingId PMSchedulingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PMSchedulingStatus Status { get; private set; }

    // ---- PMScheduling-specific business properties --------------------------------

    /// <summary>EquipmentId business attribute.</summary>
    public Guid EquipmentId { get; private set; }

    /// <summary>EquipmentDescription business attribute.</summary>
    public string EquipmentDescription { get; private set; }

    /// <summary>PmTaskType business attribute.</summary>
    public string PmTaskType { get; private set; }

    /// <summary>DueAtUtc business attribute.</summary>
    public UtcTimestamp? DueAtUtc { get; private set; }

    /// <summary>IsOverdue business attribute.</summary>
    public bool IsOverdue { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPMSchedulingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPMSchedulingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPMSchedulingDomainEvent>(_domainEvents);

    // For rehydration only.
    private PMScheduling() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        EquipmentDescription = null!;
        PmTaskType = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PMScheduling aggregate and raises <see cref="PMSchedulingCreatedEvent"/>.</summary>
    public static PMScheduling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid equipmentId = default,
        string equipmentDescription = "",
        string pmTaskType = "",
        UtcTimestamp? dueAtUtc = null,
        bool isOverdue = false)
    {
        var agg = new PMScheduling
        {
            PMSchedulingId     = PMSchedulingId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PMSchedulingStatus.Draft,
            EquipmentId = equipmentId,
            EquipmentDescription = (equipmentDescription ?? string.Empty).Trim(),
            PmTaskType = (pmTaskType ?? string.Empty).Trim(),
            DueAtUtc = dueAtUtc,
            IsOverdue = isOverdue,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PMSchedulingCreatedEvent(
            EventId:       new PMSchedulingEventId(Uuid7.New()),
            PMSchedulingId:      agg.PMSchedulingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Priority code (P1-P4) assigned when the task is escalated for being overdue.</summary>
    public string? EscalationPriorityCode { get; private set; }

    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; private set; }

    // ---- Lifecycle: Preventive Maintenance (PM) Scheduling -------------------

    /// <summary>Places the PM task on the PM calendar with a due date ("PM calendar", "Automated PM reminders"). Valid only from <see cref="PMSchedulingStatus.Draft"/>.</summary>
    public void SchedulePreventiveMaintenance(UtcTimestamp dueAtUtc, UtcTimestamp scheduledAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PMSchedulingStatus.Draft);

        DueAtUtc = dueAtUtc;
        IsOverdue = false;
        Status = PMSchedulingStatus.Scheduled;

        AddDomainEvent(new PMSchedulingScheduledEvent(
            EventId: new PMSchedulingEventId(Uuid7.New()),
            PMSchedulingId: PMSchedulingId,
            DueAtUtc: dueAtUtc,
            ScheduledAtUtc: scheduledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Records completion of the PM task ("PM compliance reporting"). Valid from <see cref="PMSchedulingStatus.Scheduled"/>, <see cref="PMSchedulingStatus.InProgress"/>, or <see cref="PMSchedulingStatus.Overdue"/>.</summary>
    public void RecordCompletion(string? completionNotes, UtcTimestamp completedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PMSchedulingStatus.Scheduled, PMSchedulingStatus.InProgress, PMSchedulingStatus.Overdue);

        CompletionNotes = string.IsNullOrWhiteSpace(completionNotes) ? null : completionNotes.Trim();
        IsOverdue = false;
        Status = PMSchedulingStatus.Completed;

        AddDomainEvent(new PMSchedulingCompletedEvent(
            EventId: new PMSchedulingEventId(Uuid7.New()),
            PMSchedulingId: PMSchedulingId,
            CompletionNotes: CompletionNotes,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Escalates an overdue PM task, tagging it with a Work Order Priority code (P1-P4)
    /// ("Tracking overdue PM tasks", "Automatic escalation for overdue P1/P2 items").
    /// Valid only from <see cref="PMSchedulingStatus.Scheduled"/>.
    /// </summary>
    public void Escalate(string priorityCode, UtcTimestamp escalatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PMSchedulingStatus.Scheduled);
        if (priorityCode is not ("P1" or "P2" or "P3" or "P4"))
            throw new ArgumentException("PriorityCode must be one of P1, P2, P3, P4.", nameof(priorityCode));

        EscalationPriorityCode = priorityCode;
        IsOverdue = true;
        Status = PMSchedulingStatus.Overdue;

        AddDomainEvent(new PMSchedulingEscalatedEvent(
            EventId: new PMSchedulingEventId(Uuid7.New()),
            PMSchedulingId: PMSchedulingId,
            PriorityCode: priorityCode,
            EscalatedAtUtc: escalatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Cancels a draft or scheduled PM task (e.g., equipment decommissioned).</summary>
    public void Cancel(string reason, UtcTimestamp cancelledAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PMSchedulingStatus.Draft, PMSchedulingStatus.Scheduled);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A cancellation reason must be supplied.", nameof(reason));

        Status = PMSchedulingStatus.Cancelled;

        AddDomainEvent(new PMSchedulingCancelledEvent(
            EventId: new PMSchedulingEventId(Uuid7.New()),
            PMSchedulingId: PMSchedulingId,
            Reason: reason.Trim(),
            CancelledAtUtc: cancelledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPMSchedulingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PMSchedulingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PMScheduling {PMSchedulingId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params PMSchedulingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new PMSchedulingInvalidStatusTransitionException(
            PMSchedulingId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}