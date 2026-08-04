// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real PublicAlertBroadcast lifecycle: sent,
//                  delivery confirmed, failed, and cancelled. Feeds the broadcast delivery
//                  success rate KPI.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Events;

/// <summary>Raised when a public alert broadcast is sent to its target channel.</summary>
public sealed record PublicAlertBroadcastSentEvent(
    PublicAlertBroadcastEventId EventId,
    PublicAlertBroadcastId PublicAlertBroadcastId,
    string SeverityCode,
    string ChannelCode,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicAlertBroadcastDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PublicAlertBroadcastSent";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when delivery of the broadcast is confirmed by the target channel.</summary>
public sealed record PublicAlertBroadcastDeliveryConfirmedEvent(
    PublicAlertBroadcastEventId EventId,
    PublicAlertBroadcastId PublicAlertBroadcastId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicAlertBroadcastDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PublicAlertBroadcastDeliveryConfirmed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when delivery of the broadcast to its target channel fails.</summary>
public sealed record PublicAlertBroadcastFailedEvent(
    PublicAlertBroadcastEventId EventId,
    PublicAlertBroadcastId PublicAlertBroadcastId,
    string FailureReason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicAlertBroadcastDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PublicAlertBroadcastFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a broadcast is cancelled.</summary>
public sealed record PublicAlertBroadcastCancelledEvent(
    PublicAlertBroadcastEventId EventId,
    PublicAlertBroadcastId PublicAlertBroadcastId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPublicAlertBroadcastDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PublicAlertBroadcastCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
