// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardPublishedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a draft Dashboard is published and becomes visible to its
//                  permitted role(s).
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardEventId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardEventId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Events;

/// <summary>Raised when a <see cref="Aggregates.Dashboard"/> is published.</summary>
public sealed record DashboardPublishedEvent(
    DashboardEventId EventId,
    DashboardId      DashboardId,
    UtcTimestamp     PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}
