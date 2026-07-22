// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertEscalatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when an Alert is escalated, either manually or automatically because
//                  it was not acknowledged within its severity's escalation threshold.
//                  Additive-only evolution.
// ============================================================================
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;
using AlertEventId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertEventId;

namespace PlantSafety.Domain.Alerts.Alert.Events;

/// <summary>Raised when an <see cref="Aggregates.Alert"/> is escalated.</summary>
public sealed record AlertEscalatedEvent(
    AlertEventId  EventId,
    AlertId       AlertId,
    string        EscalationReason,
    bool          WasAutoEscalated,
    UtcTimestamp  EscalatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AlertEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
