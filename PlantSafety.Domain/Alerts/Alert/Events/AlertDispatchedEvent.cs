// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertDispatchedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a response has been dispatched for an Alert (first
//                  responders/maintenance/security notified and en route).
//                  Additive-only evolution.
// ============================================================================
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;
using AlertEventId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertEventId;

namespace PlantSafety.Domain.Alerts.Alert.Events;

/// <summary>Raised when a response is dispatched for an <see cref="Aggregates.Alert"/>.</summary>
public sealed record AlertDispatchedEvent(
    AlertEventId  EventId,
    AlertId       AlertId,
    string        DispatchedTo,
    UtcTimestamp  DispatchedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AlertDispatched";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DispatchedAtUtc;
}
