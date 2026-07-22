// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetConfiguredEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a Widget's configuration (data source or parameters) is updated,
//                  per "3. Dashboard Service Layer: Supports widget configuration (time range,
//                  filters, thresholds)".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Events;

/// <summary>Raised when a <see cref="Aggregates.Widget"/> is (re)configured.</summary>
public sealed record WidgetConfiguredEvent(
    WidgetEventId EventId,
    WidgetId      WidgetId,
    UtcTimestamp  ConfiguredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IWidgetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WidgetConfigured";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ConfiguredAtUtc;
}
