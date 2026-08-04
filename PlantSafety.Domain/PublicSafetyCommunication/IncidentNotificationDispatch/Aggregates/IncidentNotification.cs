// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single outbound notification dispatched to an external
//                  public-safety agency (fire, police, EMS, regulatory hotline) during or after a
//                  safety event. Models the real dispatch lifecycle: dispatch, acknowledgement,
//                  closure, and delivery failure. DispatchedAtUtc/AcknowledgedAtUtc are retained
//                  as EPCRA Section 304 emergency-release notification-timing compliance evidence.
//                  Unacknowledged/failed notifications feed the RiskManagement escalation hook
//                  (see IncidentNotificationEscalationPolicy).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates;

/// <summary>Aggregate root for a single outbound notification dispatched to an external public-safety agency.</summary>
public sealed partial class IncidentNotification
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this IncidentNotification aggregate.</summary>
    public IncidentNotificationId IncidentNotificationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current dispatch lifecycle status.</summary>
    public IncidentNotificationStatus Status { get; private set; }

    // ---- Dispatch identity ---------------------------------------------------------

    /// <summary>Identity of the originating SafetyEvents.EmergencyManagement incident, if any.</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Identity of the receiving PublicSafetyAgency.</summary>
    public PublicSafetyAgencyId PublicSafetyAgencyId { get; private set; }

    /// <summary>Channel used to deliver this notification.</summary>
    public PublicSafetyDeliveryChannel Channel { get; private set; } = PublicSafetyDeliveryChannel.Default;

    /// <summary>Notification message content sent to the agency.</summary>
    public string Message { get; private set; } = string.Empty;

    // ---- Timing (EPCRA Section 304 compliance evidence) ----------------------------

    /// <summary>UTC timestamp the notification was dispatched.</summary>
    public UtcTimestamp? DispatchedAtUtc { get; private set; }

    /// <summary>Identity of the user who dispatched the notification.</summary>
    public UserId? DispatchedBy { get; private set; }

    /// <summary>UTC timestamp the agency acknowledged receipt, if any.</summary>
    public UtcTimestamp? AcknowledgedAtUtc { get; private set; }

    /// <summary>Name/desk of the agency contact who acknowledged receipt.</summary>
    public string? AcknowledgedByContactName { get; private set; }

    /// <summary>UTC timestamp the notification lifecycle was closed, if any.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>Reason delivery failed, if the notification is in the Failed status.</summary>
    public string? FailureReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IIncidentNotificationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IIncidentNotificationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IIncidentNotificationDomainEvent>(_domainEvents);

    // For rehydration only.
    private IncidentNotification() { }

    // ---- Factory: Create (Dispatch) -----------------------------------------------------------

    /// <summary>Creates and immediately dispatches a new IncidentNotification, raising <see cref="IncidentNotificationDispatchedEvent"/>.</summary>
    public static IncidentNotification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        PublicSafetyAgencyId publicSafetyAgencyId,
        PublicSafetyDeliveryChannel channel,
        string message)
    {
        var agg = new IncidentNotification
        {
            IncidentNotificationId = IncidentNotificationId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = IncidentNotificationStatus.Dispatched,
            IncidentId               = incidentId,
            PublicSafetyAgencyId     = publicSafetyAgencyId,
            Channel                  = channel,
            Message                  = (message ?? string.Empty).Trim(),
            DispatchedAtUtc          = createdAtUtc,
            DispatchedBy             = createdBy,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new IncidentNotificationDispatchedEvent(
            IncidentNotificationEventId.New(), agg.IncidentNotificationId, publicSafetyAgencyId, channel.Code, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Acknowledge -----------------------------------------------

    /// <summary>Records the receiving agency's acknowledgement of this notification.</summary>
    public void Acknowledge(string acknowledgedByContactName, UtcTimestamp acknowledgedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, IncidentNotificationStatus.Dispatched);

        Status = IncidentNotificationStatus.Acknowledged;
        AcknowledgedAtUtc = acknowledgedAtUtc;
        AcknowledgedByContactName = (acknowledgedByContactName ?? string.Empty).Trim();

        AddDomainEvent(new IncidentNotificationAcknowledgedEvent(
            IncidentNotificationEventId.New(), IncidentNotificationId, AcknowledgedByContactName, acknowledgedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------

    /// <summary>Closes the notification lifecycle.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, IncidentNotificationStatus.Dispatched, IncidentNotificationStatus.Acknowledged);

        Status = IncidentNotificationStatus.Closed;
        ClosedAtUtc = closedAtUtc;

        AddDomainEvent(new IncidentNotificationClosedEvent(
            IncidentNotificationEventId.New(), IncidentNotificationId, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordDeliveryFailure (risk hook) -----------------------------------------------

    /// <summary>Records a delivery failure for this notification, feeding the RiskManagement escalation hook.</summary>
    public void RecordDeliveryFailure(string failureReason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        Status = IncidentNotificationStatus.Failed;
        FailureReason = (failureReason ?? string.Empty).Trim();

        AddDomainEvent(new IncidentNotificationDeliveryFailedEvent(
            IncidentNotificationEventId.New(), IncidentNotificationId, FailureReason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IIncidentNotificationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params IncidentNotificationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new IncidentNotificationInvalidStatusTransitionException(IncidentNotificationId, Status.ToString(), string.Join("|", allowed), correlationId, causationId);
    }
}
