// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetection.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the AutomatedIncidentDetection bounded context (module: Observability).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates;

/// <summary>Aggregate root for the AutomatedIncidentDetection bounded context.</summary>
public sealed partial class AutomatedIncidentDetection
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this AutomatedIncidentDetection aggregate.</summary>
    public AutomatedIncidentDetectionId AutomatedIncidentDetectionId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AutomatedIncidentDetectionStatus Status { get; private set; }

    // ---- AutomatedIncidentDetection-specific business properties --------------------------------

    /// <summary>DetectionType business attribute.</summary>
    public string DetectionType { get; private set; }

    /// <summary>LocationDescription business attribute.</summary>
    public string LocationDescription { get; private set; }

    /// <summary>NotifiedSupervisorUserId business attribute.</summary>
    public UserId? NotifiedSupervisorUserId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAutomatedIncidentDetectionDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAutomatedIncidentDetectionDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAutomatedIncidentDetectionDomainEvent>(_domainEvents);

    // For rehydration only.
    private AutomatedIncidentDetection() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new AutomatedIncidentDetection aggregate and raises <see cref="AutomatedIncidentDetectionCreatedEvent"/>.</summary>
    public static AutomatedIncidentDetection Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string detectionType = "",
        string locationDescription = "",
        UserId? notifiedSupervisorUserId = null)
    {
        var agg = new AutomatedIncidentDetection
        {
            AutomatedIncidentDetectionId     = AutomatedIncidentDetectionId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = AutomatedIncidentDetectionStatus.Detected,
            DetectionType = (detectionType ?? string.Empty).Trim(),
            LocationDescription = (locationDescription ?? string.Empty).Trim(),
            NotifiedSupervisorUserId = notifiedSupervisorUserId,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new AutomatedIncidentDetectionCreatedEvent(
            EventId:       new AutomatedIncidentDetectionEventId(Uuid7.New()),
            AutomatedIncidentDetectionId:      agg.AutomatedIncidentDetectionId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Identity of the user who acknowledged the detection, if any.</summary>
    public UserId? AcknowledgedBy { get; private set; }

    /// <summary>Reference identifier of the formal Incident Management record this detection was escalated into, if any.</summary>
    public Guid? IncidentReferenceId { get; private set; }

    /// <summary>Reason recorded when the detection is dismissed as a false positive.</summary>
    public string? DismissalReason { get; private set; }

    // ---- Lifecycle: Automated Incident Detection ------------------------------

    /// <summary>Instantly notifies the assigned supervisor of the detection ("Instant notifications to supervisors"). Valid only from <see cref="AutomatedIncidentDetectionStatus.Detected"/>.</summary>
    public void NotifySupervisor(UserId supervisorUserId, UtcTimestamp notifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AutomatedIncidentDetectionStatus.Detected);

        NotifiedSupervisorUserId = supervisorUserId;
        Status = AutomatedIncidentDetectionStatus.SupervisorNotified;

        AddDomainEvent(new AutomatedIncidentDetectionSupervisorNotifiedEvent(
            EventId: new AutomatedIncidentDetectionEventId(Uuid7.New()),
            AutomatedIncidentDetectionId: AutomatedIncidentDetectionId,
            NotifiedSupervisorUserId: supervisorUserId,
            NotifiedAtUtc: notifiedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Acknowledges the detection and begins evidence review. Valid only from <see cref="AutomatedIncidentDetectionStatus.SupervisorNotified"/>.</summary>
    public void Acknowledge(UserId acknowledgedBy, UtcTimestamp acknowledgedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AutomatedIncidentDetectionStatus.SupervisorNotified);

        AcknowledgedBy = acknowledgedBy;
        Status = AutomatedIncidentDetectionStatus.Acknowledged;

        AddDomainEvent(new AutomatedIncidentDetectionAcknowledgedEvent(
            EventId: new AutomatedIncidentDetectionEventId(Uuid7.New()),
            AutomatedIncidentDetectionId: AutomatedIncidentDetectionId,
            AcknowledgedBy: acknowledgedBy,
            AcknowledgedAtUtc: acknowledgedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Escalates a confirmed detection into a formal Incident Management record. Valid only from <see cref="AutomatedIncidentDetectionStatus.Acknowledged"/>.</summary>
    public void EscalateToIncident(Guid incidentReferenceId, UtcTimestamp escalatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AutomatedIncidentDetectionStatus.Acknowledged);
        if (incidentReferenceId == Guid.Empty)
            throw new ArgumentException("IncidentReferenceId must not be empty.", nameof(incidentReferenceId));

        IncidentReferenceId = incidentReferenceId;
        Status = AutomatedIncidentDetectionStatus.EscalatedToIncident;

        AddDomainEvent(new AutomatedIncidentDetectionEscalatedToIncidentEvent(
            EventId: new AutomatedIncidentDetectionEventId(Uuid7.New()),
            AutomatedIncidentDetectionId: AutomatedIncidentDetectionId,
            IncidentReferenceId: incidentReferenceId,
            EscalatedAtUtc: escalatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Dismisses the detection as a false positive. Valid from <see cref="AutomatedIncidentDetectionStatus.Detected"/>, <see cref="AutomatedIncidentDetectionStatus.SupervisorNotified"/>, or <see cref="AutomatedIncidentDetectionStatus.Acknowledged"/>.</summary>
    public void Dismiss(string reason, UtcTimestamp dismissedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(
            correlationId, causationId,
            AutomatedIncidentDetectionStatus.Detected,
            AutomatedIncidentDetectionStatus.SupervisorNotified,
            AutomatedIncidentDetectionStatus.Acknowledged);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A dismissal reason must be supplied.", nameof(reason));

        DismissalReason = reason.Trim();
        Status = AutomatedIncidentDetectionStatus.Dismissed;

        AddDomainEvent(new AutomatedIncidentDetectionDismissedEvent(
            EventId: new AutomatedIncidentDetectionEventId(Uuid7.New()),
            AutomatedIncidentDetectionId: AutomatedIncidentDetectionId,
            Reason: DismissalReason,
            DismissedAtUtc: dismissedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAutomatedIncidentDetectionDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AutomatedIncidentDetectionStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AutomatedIncidentDetection {AutomatedIncidentDetectionId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params AutomatedIncidentDetectionStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new AutomatedIncidentDetectionInvalidStatusTransitionException(
            AutomatedIncidentDetectionId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}