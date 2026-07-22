// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new Dashboard aggregate is created.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardEventId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardEventId;
using DashboardCategory = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardCategory;

namespace PlantSafety.Domain.Dashboards.Dashboard.Events;

/// <summary>Raised when a new <see cref="Aggregates.Dashboard"/> is created.</summary>
public sealed record DashboardCreatedEvent(
    DashboardEventId EventId,
    DashboardId      DashboardId,
    DashboardCategory Category,
    UtcTimestamp     CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
