// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Dashboard aggregate.
//                  Additive-only evolution.
// ============================================================================
using DashboardStatus = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardStatus;

namespace PlantSafety.Domain.Dashboards.Dashboard.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.Dashboard"/> aggregate.</summary>
public static class DashboardLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the dashboard may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(DashboardStatus current) => current == DashboardStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the dashboard may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(DashboardStatus current) => current is DashboardStatus.Draft or DashboardStatus.Published;
}
