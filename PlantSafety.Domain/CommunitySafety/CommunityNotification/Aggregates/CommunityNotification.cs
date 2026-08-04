// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the CommunityNotification bounded context (Outreach &amp;
//                  Notification History subdomain of CommunitySafety). Represents a single
//                  outbound community notification sent to a registered CommunityContact (weak
//                  Guid reference), tracking delivery channel, send time, and acknowledgement for
//                  the notification reach/acknowledgement-rate KPI. Also carries the reserved
//                  integration point for a future PublicSafetyCommunication module via
//                  CommunityNotificationChannel.ExternalPublicSafetyPlatform and the
//                  CommunityNotificationSentEvent. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Events;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Exceptions;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Rules;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Aggregates;

/// <summary>Aggregate root for the CommunityNotification bounded context.</summary>
public sealed partial class CommunityNotification
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CommunityNotification aggregate.</summary>
    public CommunityNotificationId CommunityNotificationId { get; private set; }

    /// <summary>Tenant that owns this notification record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the notification relates to, or null when tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CommunityNotificationStatus Status { get; private set; }

    // ---- Notification-specific business properties ------------------------------

    /// <summary>Delivery channel used to send this notification.</summary>
    public CommunityNotificationChannel Channel { get; private set; }

    /// <summary>Weak reference to the recipient CommunityContact.</summary>
    public Guid RecipientContactId { get; private set; }

    /// <summary>Notification subject line.</summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>Notification message body.</summary>
    public string MessageBody { get; private set; } = string.Empty;

    /// <summary>Optional weak reference to the CommunityConcern this notification relates to, if any.</summary>
    public Guid? RelatedConcernId { get; private set; }

    /// <summary>Optional weak reference to the SafetyEvents incident this notification relates to, if any.</summary>
    public Guid? RelatedIncidentId { get; private set; }

    /// <summary>UTC instant the notification was sent.</summary>
    public UtcTimestamp SentAtUtc { get; private set; }

    /// <summary>UTC instant the recipient acknowledged the notification, if acknowledged.</summary>
    public UtcTimestamp? AcknowledgedAtUtc { get; private set; }

    /// <summary>Reason recorded for delivery failure, if failed.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICommunityNotificationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICommunityNotificationDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICommunityNotificationDomainEvent>(_domainEvents);

    // For rehydration only.
    private CommunityNotification() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Sends a new CommunityNotification aggregate and raises
    /// <see cref="CommunityNotificationSentEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static CommunityNotification Send(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        CommunityNotificationChannel channel,
        Guid recipientContactId,
        string subject,
        string messageBody,
        Guid? relatedConcernId = null,
        Guid? relatedIncidentId = null)
    {
        var normalizedSubject = (subject ?? string.Empty).Trim();
        var normalizedBody = (messageBody ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (recipientContactId == Guid.Empty)
            throw new ArgumentException("RecipientContactId is required.", nameof(recipientContactId));
        if (string.IsNullOrWhiteSpace(normalizedSubject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(normalizedBody))
            throw new ArgumentException("MessageBody is required.", nameof(messageBody));

        var agg = new CommunityNotification
        {
            CommunityNotificationId = CommunityNotificationId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = CommunityNotificationStatus.Sent,
            Channel = channel,
            RecipientContactId = recipientContactId,
            Subject = normalizedSubject,
            MessageBody = normalizedBody,
            RelatedConcernId = relatedConcernId,
            RelatedIncidentId = relatedIncidentId,
            SentAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new CommunityNotificationSentEvent(
            CommunityNotificationEventId.New(), agg.CommunityNotificationId, agg.Channel,
            agg.RecipientContactId, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Reconstructs a CommunityNotification aggregate from previously persisted state, without raising domain events.</summary>
    public static CommunityNotification Rehydrate(
        CommunityNotificationId communityNotificationId,
        Guid tenantId,
        Guid? siteId,
        CommunityNotificationStatus status,
        CommunityNotificationChannel channel,
        Guid recipientContactId,
        string subject,
        string messageBody,
        Guid? relatedConcernId,
        Guid? relatedIncidentId,
        UtcTimestamp sentAtUtc,
        UtcTimestamp? acknowledgedAtUtc,
        string? failureReason,
        UtcTimestamp createdAtUtc,
        UserId createdBy)
        => new()
        {
            CommunityNotificationId = communityNotificationId,
            TenantId = tenantId,
            SiteId = siteId,
            Status = status,
            Channel = channel,
            RecipientContactId = recipientContactId,
            Subject = subject,
            MessageBody = messageBody,
            RelatedConcernId = relatedConcernId,
            RelatedIncidentId = relatedIncidentId,
            SentAtUtc = sentAtUtc,
            AcknowledgedAtUtc = acknowledgedAtUtc,
            FailureReason = failureReason,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

    // ---- Lifecycle: Acknowledge -------------------------------------------------

    /// <summary>Records recipient acknowledgement. Raises <see cref="CommunityNotificationAcknowledgedEvent"/>.</summary>
    /// <exception cref="CommunityNotificationInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Acknowledge(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityNotificationLifecycleRules.CanTransition(Status, CommunityNotificationStatus.Acknowledged))
            throw new CommunityNotificationInvalidStatusTransitionException(
                CommunityNotificationId, Status.ToString(), CommunityNotificationStatus.Acknowledged.ToString(), correlationId, causationId);

        Status = CommunityNotificationStatus.Acknowledged;
        AcknowledgedAtUtc = nowUtc;

        AddDomainEvent(new CommunityNotificationAcknowledgedEvent(
            CommunityNotificationEventId.New(), CommunityNotificationId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: MarkFailed --------------------------------------------------

    /// <summary>Marks the notification delivery as failed.</summary>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is empty.</exception>
    /// <exception cref="CommunityNotificationInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void MarkFailed(string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityNotificationLifecycleRules.CanTransition(Status, CommunityNotificationStatus.Failed))
            throw new CommunityNotificationInvalidStatusTransitionException(
                CommunityNotificationId, Status.ToString(), CommunityNotificationStatus.Failed.ToString(), correlationId, causationId);

        var normalizedReason = (reason ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
            throw new ArgumentException("Reason is required to mark a notification as failed.", nameof(reason));

        Status = CommunityNotificationStatus.Failed;
        FailureReason = normalizedReason;
    }

    // ---- Lifecycle: Archive ---------------------------------------------------

    /// <summary>Archives the notification for long-term retention.</summary>
    /// <exception cref="CommunityNotificationInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityNotificationLifecycleRules.CanTransition(Status, CommunityNotificationStatus.Archived))
            throw new CommunityNotificationInvalidStatusTransitionException(
                CommunityNotificationId, Status.ToString(), CommunityNotificationStatus.Archived.ToString(), correlationId, causationId);

        Status = CommunityNotificationStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICommunityNotificationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
