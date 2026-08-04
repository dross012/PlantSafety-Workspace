// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcast.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single public alert broadcast sent to public warning
//                  systems (EAS/WEA/IPAWS, siren networks, social media) during a safety event.
//                  Models the real broadcast lifecycle: send, delivery confirmation, failure, and
//                  cancellation. Feeds the broadcast delivery success rate KPI.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates;

/// <summary>Aggregate root for a single public alert broadcast sent to public warning systems.</summary>
public sealed partial class PublicAlertBroadcast
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this PublicAlertBroadcast aggregate.</summary>
    public PublicAlertBroadcastId PublicAlertBroadcastId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current delivery lifecycle status.</summary>
    public PublicAlertBroadcastStatus Status { get; private set; }

    // ---- Broadcast content -----------------------------------------------------

    /// <summary>Identity of the originating SafetyEvents.EmergencyManagement incident, if any.</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Severity level of this broadcast (CAP-aligned).</summary>
    public PublicAlertSeverity Severity { get; private set; } = PublicAlertSeverity.Default;

    /// <summary>Target broadcast channel.</summary>
    public PublicBroadcastChannel Channel { get; private set; } = PublicBroadcastChannel.Default;

    /// <summary>Short headline for the alert.</summary>
    public string Headline { get; private set; } = string.Empty;

    /// <summary>Full alert message content.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Description of the geographic area targeted by this broadcast.</summary>
    public string TargetArea { get; private set; } = string.Empty;

    // ---- Timing -------------------------------------------------------------------

    /// <summary>UTC timestamp the broadcast was sent.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>Identity of the user who issued the broadcast.</summary>
    public UserId? IssuedBy { get; private set; }

    /// <summary>UTC timestamp delivery was confirmed, if any.</summary>
    public UtcTimestamp? DeliveryConfirmedAtUtc { get; private set; }

    /// <summary>Reason delivery failed, if the broadcast is in the Failed status.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>UTC timestamp the broadcast was cancelled, if any.</summary>
    public UtcTimestamp? CancelledAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPublicAlertBroadcastDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPublicAlertBroadcastDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPublicAlertBroadcastDomainEvent>(_domainEvents);

    // For rehydration only.
    private PublicAlertBroadcast() { }

    // ---- Factory: Create (Broadcast) -----------------------------------------------------------

    /// <summary>Creates and immediately sends a new PublicAlertBroadcast, raising <see cref="PublicAlertBroadcastSentEvent"/>.</summary>
    public static PublicAlertBroadcast Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        PublicAlertSeverity severity,
        PublicBroadcastChannel channel,
        string headline,
        string message,
        string targetArea)
    {
        var agg = new PublicAlertBroadcast
        {
            PublicAlertBroadcastId = PublicAlertBroadcastId.New(),
            TenantId    = tenantId,
            SiteId      = siteId,
            Status      = PublicAlertBroadcastStatus.Sent,
            IncidentId  = incidentId,
            Severity    = severity,
            Channel     = channel,
            Headline    = (headline ?? string.Empty).Trim(),
            Message     = (message ?? string.Empty).Trim(),
            TargetArea  = (targetArea ?? string.Empty).Trim(),
            IssuedAtUtc = createdAtUtc,
            IssuedBy    = createdBy,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new PublicAlertBroadcastSentEvent(
            PublicAlertBroadcastEventId.New(), agg.PublicAlertBroadcastId, severity.Code, channel.Code, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ConfirmDelivery -----------------------------------------------

    /// <summary>Confirms delivery of this broadcast to its target channel.</summary>
    public void ConfirmDelivery(UtcTimestamp confirmedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PublicAlertBroadcastStatus.Sent);

        Status = PublicAlertBroadcastStatus.Delivered;
        DeliveryConfirmedAtUtc = confirmedAtUtc;

        AddDomainEvent(new PublicAlertBroadcastDeliveryConfirmedEvent(
            PublicAlertBroadcastEventId.New(), PublicAlertBroadcastId, confirmedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordFailure -----------------------------------------------

    /// <summary>Records that delivery of this broadcast to its target channel failed.</summary>
    public void RecordFailure(string failureReason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        Status = PublicAlertBroadcastStatus.Failed;
        FailureReason = (failureReason ?? string.Empty).Trim();

        AddDomainEvent(new PublicAlertBroadcastFailedEvent(
            PublicAlertBroadcastEventId.New(), PublicAlertBroadcastId, FailureReason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Cancel -----------------------------------------------

    /// <summary>Cancels the broadcast.</summary>
    public void Cancel(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status is PublicAlertBroadcastStatus.Cancelled)
            throw new PublicAlertBroadcastInvalidStatusTransitionException(PublicAlertBroadcastId, Status.ToString(), PublicAlertBroadcastStatus.Cancelled.ToString(), correlationId, causationId);

        Status = PublicAlertBroadcastStatus.Cancelled;
        CancelledAtUtc = occurredAtUtc;

        AddDomainEvent(new PublicAlertBroadcastCancelledEvent(
            PublicAlertBroadcastEventId.New(), PublicAlertBroadcastId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPublicAlertBroadcastDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params PublicAlertBroadcastStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new PublicAlertBroadcastInvalidStatusTransitionException(PublicAlertBroadcastId, Status.ToString(), string.Join("|", allowed), correlationId, causationId);
    }
}
