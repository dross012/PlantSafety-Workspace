// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertTriggeredEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new Alert aggregate is created (triggered).
//                  Additive-only evolution.
// ============================================================================
using AlertCategory = PlantSafety.Domain.Alerts.Alert.Enums.AlertCategory;
using AlertSeverity = PlantSafety.Domain.Alerts.Alert.Enums.AlertSeverity;
using AlertSourceType = PlantSafety.Domain.Alerts.Alert.Enums.AlertSourceType;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;
using AlertEventId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertEventId;

namespace PlantSafety.Domain.Alerts.Alert.Events;

/// <summary>Raised when a new <see cref="Aggregates.Alert"/> is triggered.</summary>
public sealed record AlertTriggeredEvent(
    AlertEventId    EventId,
    AlertId         AlertId,
    AlertCategory   Category,
    AlertSeverity   Severity,
    AlertSourceType SourceType,
    UtcTimestamp    TriggeredAtUtc,
    PerformedByActor Actor,
    CorrelationId   CorrelationId,
    CausationId     CausationId
) : IAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AlertTriggered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => TriggeredAtUtc;
}
