// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardArchivedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a Dashboard is archived.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardEventId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardEventId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Events;

/// <summary>Raised when a <see cref="Aggregates.Dashboard"/> is archived.</summary>
public sealed record DashboardArchivedEvent(
    DashboardEventId EventId,
    DashboardId      DashboardId,
    UtcTimestamp     ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
