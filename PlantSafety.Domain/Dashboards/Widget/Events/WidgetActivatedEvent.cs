// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetActivatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a draft Widget is activated and becomes available for placement
//                  on dashboards.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Events;

/// <summary>Raised when a <see cref="Aggregates.Widget"/> is activated.</summary>
public sealed record WidgetActivatedEvent(
    WidgetEventId EventId,
    WidgetId      WidgetId,
    UtcTimestamp  ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IWidgetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WidgetActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}
