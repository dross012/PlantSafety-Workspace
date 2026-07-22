// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardWidgetAddedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a Widget is added to a Dashboard's layout, per "6. Extensibility:
//                  Add/remove widgets".
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardEventId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardEventId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Events;

/// <summary>Raised when a Widget is added to a <see cref="Aggregates.Dashboard"/>'s layout.</summary>
public sealed record DashboardWidgetAddedEvent(
    DashboardEventId EventId,
    DashboardId      DashboardId,
    Guid             WidgetId,
    UtcTimestamp     AddedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardWidgetAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AddedAtUtc;
}
