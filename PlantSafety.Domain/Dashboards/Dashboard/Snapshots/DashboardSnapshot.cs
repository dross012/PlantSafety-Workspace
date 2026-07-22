// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Dashboard aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardStatus = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardStatus;
using DashboardCategory = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardCategory;

namespace PlantSafety.Domain.Dashboards.Dashboard.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Aggregates.Dashboard"/> aggregate for fast rehydration.</summary>
public sealed record DashboardSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DashboardId DashboardId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DashboardStatus Status { get; init; }

    /// <summary>Category code at snapshot time.</summary>
    public string CategoryCode { get; init; } = DashboardCategory.Default.Code;

    /// <summary>Widget count at snapshot time.</summary>
    public int WidgetCount { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }
}
