// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertDeliveryRecordedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a multi-channel delivery attempt for an Alert is recorded, per
//                  "12. Multi-Channel Alert Delivery".
//                  Additive-only evolution.
// ============================================================================
using AlertDeliveryChannel = PlantSafety.Domain.Alerts.Alert.Enums.AlertDeliveryChannel;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;
using AlertEventId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertEventId;

namespace PlantSafety.Domain.Alerts.Alert.Events;

/// <summary>Raised when a delivery attempt for an <see cref="Aggregates.Alert"/> is recorded.</summary>
public sealed record AlertDeliveryRecordedEvent(
    AlertEventId        EventId,
    AlertId             AlertId,
    AlertDeliveryChannel Channel,
    bool                DeliverySucceeded,
    UtcTimestamp        RecordedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AlertDeliveryRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}
