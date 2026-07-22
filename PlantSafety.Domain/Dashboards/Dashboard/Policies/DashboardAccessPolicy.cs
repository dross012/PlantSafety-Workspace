// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardAccessPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy determining whether a viewer role may access a Dashboard, per
//                  "4. Role-Based Dashboards: Permissions are enforced at dashboard and widget
//                  level." Executives may additionally view every dashboard (Full Program Master
//                  Dashboard aggregates all data).
//                  Additive-only evolution.
// ============================================================================
using DashboardStatus = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardStatus;
using DashboardRole = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardRole;

namespace PlantSafety.Domain.Dashboards.Dashboard.Policies;

/// <summary>Determines whether a given viewer role may access a published <see cref="Aggregates.Dashboard"/>.</summary>
public static class DashboardAccessPolicy
{
    /// <summary>
    /// Returns <see langword="true"/> when a viewer with <paramref name="viewerRole"/> may access
    /// the dashboard: the dashboard must be published, and the viewer role must match the
    /// dashboard's primary role or be <see cref="DashboardRole.Executive"/>.
    /// </summary>
    public static bool CanAccess(Aggregates.Dashboard aggregate, DashboardRole viewerRole)
        => aggregate.Status == DashboardStatus.Published
           && (aggregate.Role == viewerRole || viewerRole == DashboardRole.Executive);
}
