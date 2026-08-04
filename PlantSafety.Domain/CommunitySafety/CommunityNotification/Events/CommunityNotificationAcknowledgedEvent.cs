// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationAcknowledgedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a recipient acknowledges receipt of a CommunityNotification. Feeds
//                  the notification reach/acknowledgement-rate KPI. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Events;

/// <summary>Raised when a <see cref="Aggregates.CommunityNotification"/> is acknowledged by its recipient.</summary>
public sealed record CommunityNotificationAcknowledgedEvent(
    CommunityNotificationEventId EventId,
    CommunityNotificationId      CommunityNotificationId,
    UtcTimestamp                  AcknowledgedAtUtc,
    PerformedByActor               Actor,
    CorrelationId                  CorrelationId,
    CausationId                    CausationId
) : ICommunityNotificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityNotificationAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}
