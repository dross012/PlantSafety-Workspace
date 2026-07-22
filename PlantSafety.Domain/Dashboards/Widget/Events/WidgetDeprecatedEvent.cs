// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetDeprecatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a Widget is deprecated.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Events;

/// <summary>Raised when a <see cref="Aggregates.Widget"/> is deprecated.</summary>
public sealed record WidgetDeprecatedEvent(
    WidgetEventId EventId,
    WidgetId      WidgetId,
    string        Reason,
    UtcTimestamp  DeprecatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IWidgetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WidgetDeprecated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeprecatedAtUtc;
}
