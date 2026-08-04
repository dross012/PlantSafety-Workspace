// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationSentEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CommunityNotification aggregate is sent. This is also the
//                  reserved integration point for a future PublicSafetyCommunication module (which
//                  may not yet exist in this repository state): that module, if/when introduced,
//                  is expected to subscribe to this event type to fan out community notifications
//                  across broader public-safety channels. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Events;

/// <summary>Raised when a new <see cref="Aggregates.CommunityNotification"/> aggregate is sent.</summary>
public sealed record CommunityNotificationSentEvent(
    CommunityNotificationEventId EventId,
    CommunityNotificationId      CommunityNotificationId,
    CommunityNotificationChannel Channel,
    Guid                          RecipientContactId,
    UtcTimestamp                  SentAtUtc,
    PerformedByActor               Actor,
    CorrelationId                  CorrelationId,
    CausationId                    CausationId
) : ICommunityNotificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityNotificationSent";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SentAtUtc;
}
