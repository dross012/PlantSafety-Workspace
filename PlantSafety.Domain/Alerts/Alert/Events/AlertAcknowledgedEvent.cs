// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertAcknowledgedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when an Alert is acknowledged by a responder.
//                  Additive-only evolution.
// ============================================================================
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;
using AlertEventId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertEventId;

namespace PlantSafety.Domain.Alerts.Alert.Events;

/// <summary>Raised when an <see cref="Aggregates.Alert"/> is acknowledged.</summary>
public sealed record AlertAcknowledgedEvent(
    AlertEventId  EventId,
    AlertId       AlertId,
    UtcTimestamp  AcknowledgedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AlertAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}
