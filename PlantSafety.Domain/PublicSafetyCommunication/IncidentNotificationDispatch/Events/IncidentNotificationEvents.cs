// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real IncidentNotification dispatch lifecycle:
//                  dispatch, acknowledgement, closure, and delivery failure. Dispatch/acknowledge
//                  timestamps double as EPCRA Section 304 compliance evidence (notification
//                  timing) per the module's compliance hook.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Events;

/// <summary>Raised when a notification is dispatched to an external public-safety agency.</summary>
public sealed record IncidentNotificationDispatchedEvent(
    IncidentNotificationEventId EventId,
    IncidentNotificationId IncidentNotificationId,
    PublicSafetyAgencyId PublicSafetyAgencyId,
    string DeliveryChannelCode,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentNotificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentNotificationDispatched";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when the receiving agency acknowledges the notification.</summary>
public sealed record IncidentNotificationAcknowledgedEvent(
    IncidentNotificationEventId EventId,
    IncidentNotificationId IncidentNotificationId,
    string AcknowledgedByContactName,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentNotificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentNotificationAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a notification's lifecycle is closed.</summary>
public sealed record IncidentNotificationClosedEvent(
    IncidentNotificationEventId EventId,
    IncidentNotificationId IncidentNotificationId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentNotificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentNotificationClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when delivery of a notification to its channel fails. Feeds the RiskManagement escalation hook.</summary>
public sealed record IncidentNotificationDeliveryFailedEvent(
    IncidentNotificationEventId EventId,
    IncidentNotificationId IncidentNotificationId,
    string FailureReason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentNotificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentNotificationDeliveryFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
