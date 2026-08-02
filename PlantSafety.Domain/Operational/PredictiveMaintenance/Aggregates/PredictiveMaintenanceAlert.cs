// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceAlert.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PredictiveMaintenanceAlert bounded context. Tracks a
//                  single predicted-equipment-failure alert from its ML/analytics-driven raise
//                  through human review, corrective action scheduling, resolution or dismissal,
//                  and closure.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at
//                  all -- only a placeholder marker class (PredictiveMaintenanceModuleMarker) and
//                  a read-only query slice over a "PredictiveMaintenanceReadModel" table existed.
//                  Field shape (EquipmentId, PredictedFailureType, Status, PredictedAtUtc,
//                  ReviewedBy) is grounded in that pre-existing read model's actual columns, not
//                  invented from scratch. Mirrors PlantSafety.Domain.Compliance.Permits.Aggregates
//                  .Permits's guarded-transition/domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.PredictiveMaintenance.Enums;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Events;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Exceptions;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Rules;
using PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates;

/// <summary>Aggregate root for the PredictiveMaintenanceAlert bounded context.</summary>
public sealed class PredictiveMaintenanceAlert
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this PredictiveMaintenanceAlert aggregate.</summary>
    public PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PredictiveMaintenanceAlertStatus Status { get; private set; }

    // ---- Alert-specific business properties -----------------------------------

    /// <summary>Identifier of the equipment being monitored.</summary>
    public Guid EquipmentId { get; private set; }

    /// <summary>Predicted failure mode/type.</summary>
    public string PredictedFailureType { get; private set; } = string.Empty;

    /// <summary>UTC instant the failure was predicted (when the alert was raised).</summary>
    public UtcTimestamp PredictedAtUtc { get; private set; }

    // ---- Lifecycle attributes (Review/ScheduleAction/Resolve/Dismiss/Close) ---

    /// <summary>Identity of the reviewer who triaged this alert.</summary>
    public UserId? ReviewedBy { get; private set; }

    /// <summary>UTC instant the alert was reviewed.</summary>
    public UtcTimestamp? ReviewedAtUtc { get; private set; }

    /// <summary>Identity of the user who scheduled corrective action.</summary>
    public UserId? ActionScheduledBy { get; private set; }

    /// <summary>Optional reference to the work order/maintenance ticket raised for this alert.</summary>
    public string? WorkOrderReference { get; private set; }

    /// <summary>UTC instant corrective action was scheduled.</summary>
    public UtcTimestamp? ActionScheduledAtUtc { get; private set; }

    /// <summary>Identity of the user who resolved this alert.</summary>
    public UserId? ResolvedBy { get; private set; }

    /// <summary>Free-text resolution notes.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>UTC instant the alert was resolved.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    /// <summary>Identity of the user who dismissed this alert.</summary>
    public UserId? DismissedBy { get; private set; }

    /// <summary>Reason recorded when an alert is dismissed without corrective action.</summary>
    public string? DismissedReason { get; private set; }

    /// <summary>UTC instant the alert was dismissed.</summary>
    public UtcTimestamp? DismissedAtUtc { get; private set; }

    /// <summary>Identity of the user who formally closed this alert.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>UTC instant the alert was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IPredictiveMaintenanceAlertDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPredictiveMaintenanceAlertDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPredictiveMaintenanceAlertDomainEvent>(_domainEvents);

    // For rehydration only.
    private PredictiveMaintenanceAlert() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Raises a new PredictiveMaintenanceAlert and records <see cref="PredictiveMaintenanceAlertRaisedEvent"/>.</summary>
    public static PredictiveMaintenanceAlert Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid equipmentId,
        string predictedFailureType,
        UtcTimestamp predictedAtUtc)
    {
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("EquipmentId must be supplied.", nameof(equipmentId));
        if (string.IsNullOrWhiteSpace(predictedFailureType))
            throw new ArgumentException("PredictedFailureType must be supplied.", nameof(predictedFailureType));

        var agg = new PredictiveMaintenanceAlert
        {
            PredictiveMaintenanceAlertId = PredictiveMaintenanceAlertId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = PredictiveMaintenanceAlertStatus.Raised,
            EquipmentId = equipmentId,
            PredictedFailureType = predictedFailureType.Trim(),
            PredictedAtUtc = predictedAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new PredictiveMaintenanceAlertRaisedEvent(
            EventId: PredictiveMaintenanceEventId.New(),
            PredictiveMaintenanceAlertId: agg.PredictiveMaintenanceAlertId,
            EquipmentId: equipmentId,
            PredictedFailureType: agg.PredictedFailureType,
            PredictedAtUtc: predictedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: Review --------------------------------------------------

    /// <summary>
    /// Records a reviewer's triage of the alert (Raised -&gt; UnderReview). Valid only from
    /// <see cref="PredictiveMaintenanceAlertStatus.Raised"/>.
    /// </summary>
    public void Review(
        UserId reviewedBy,
        UtcTimestamp reviewedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveMaintenanceAlertLifecycleRules.CanReview, PredictiveMaintenanceAlertStatus.UnderReview);

        ReviewedBy = reviewedBy;
        ReviewedAtUtc = reviewedAtUtc;
        Status = PredictiveMaintenanceAlertStatus.UnderReview;

        AddDomainEvent(new PredictiveMaintenanceAlertReviewedEvent(
            EventId: PredictiveMaintenanceEventId.New(),
            PredictiveMaintenanceAlertId: PredictiveMaintenanceAlertId,
            ReviewedBy: reviewedBy,
            ReviewedAtUtc: reviewedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: ScheduleAction -------------------------------------------

    /// <summary>
    /// Schedules corrective maintenance action against the alert (UnderReview -&gt; ActionScheduled).
    /// Valid only from <see cref="PredictiveMaintenanceAlertStatus.UnderReview"/>.
    /// </summary>
    public void ScheduleAction(
        UserId scheduledBy,
        string? workOrderReference,
        UtcTimestamp scheduledAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveMaintenanceAlertLifecycleRules.CanScheduleAction, PredictiveMaintenanceAlertStatus.ActionScheduled);

        ActionScheduledBy = scheduledBy;
        WorkOrderReference = string.IsNullOrWhiteSpace(workOrderReference) ? null : workOrderReference.Trim();
        ActionScheduledAtUtc = scheduledAtUtc;
        Status = PredictiveMaintenanceAlertStatus.ActionScheduled;

        AddDomainEvent(new PredictiveMaintenanceAlertActionScheduledEvent(
            EventId: PredictiveMaintenanceEventId.New(),
            PredictiveMaintenanceAlertId: PredictiveMaintenanceAlertId,
            ScheduledBy: scheduledBy,
            WorkOrderReference: WorkOrderReference,
            ScheduledAtUtc: scheduledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Resolve / Dismiss ----------------------------------------

    /// <summary>
    /// Records that the predicted failure has been addressed (ActionScheduled -&gt; Resolved).
    /// Valid only from <see cref="PredictiveMaintenanceAlertStatus.ActionScheduled"/>.
    /// </summary>
    public void Resolve(
        UserId resolvedBy,
        string? resolutionNotes,
        UtcTimestamp resolvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveMaintenanceAlertLifecycleRules.CanResolve, PredictiveMaintenanceAlertStatus.Resolved);

        ResolvedBy = resolvedBy;
        ResolutionNotes = string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim();
        ResolvedAtUtc = resolvedAtUtc;
        Status = PredictiveMaintenanceAlertStatus.Resolved;

        AddDomainEvent(new PredictiveMaintenanceAlertResolvedEvent(
            EventId: PredictiveMaintenanceEventId.New(),
            PredictiveMaintenanceAlertId: PredictiveMaintenanceAlertId,
            ResolvedBy: resolvedBy,
            ResolutionNotes: ResolutionNotes,
            ResolvedAtUtc: resolvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Dismisses the alert without corrective action (e.g. false positive). Valid from
    /// <see cref="PredictiveMaintenanceAlertStatus.Raised"/> or
    /// <see cref="PredictiveMaintenanceAlertStatus.UnderReview"/>.
    /// </summary>
    public void Dismiss(
        UserId dismissedBy,
        string reason,
        UtcTimestamp dismissedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveMaintenanceAlertLifecycleRules.CanDismiss, PredictiveMaintenanceAlertStatus.Dismissed);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A dismissal reason must be supplied.", nameof(reason));

        DismissedBy = dismissedBy;
        DismissedReason = reason.Trim();
        DismissedAtUtc = dismissedAtUtc;
        Status = PredictiveMaintenanceAlertStatus.Dismissed;

        AddDomainEvent(new PredictiveMaintenanceAlertDismissedEvent(
            EventId: PredictiveMaintenanceEventId.New(),
            PredictiveMaintenanceAlertId: PredictiveMaintenanceAlertId,
            DismissedBy: dismissedBy,
            Reason: DismissedReason,
            DismissedAtUtc: dismissedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ------------------------------------------------------

    /// <summary>
    /// Formally closes a resolved or dismissed alert. Valid from
    /// <see cref="PredictiveMaintenanceAlertStatus.Resolved"/> or
    /// <see cref="PredictiveMaintenanceAlertStatus.Dismissed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveMaintenanceAlertLifecycleRules.CanClose, PredictiveMaintenanceAlertStatus.Closed);

        ClosedBy = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = PredictiveMaintenanceAlertStatus.Closed;

        AddDomainEvent(new PredictiveMaintenanceAlertClosedEvent(
            EventId: PredictiveMaintenanceEventId.New(),
            PredictiveMaintenanceAlertId: PredictiveMaintenanceAlertId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPredictiveMaintenanceAlertDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<PredictiveMaintenanceAlertStatus, bool> guard, PredictiveMaintenanceAlertStatus expected)
    {
        if (guard(Status)) return;

        throw new PredictiveMaintenanceAlertInvalidStatusTransitionException(
            PredictiveMaintenanceAlertId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
