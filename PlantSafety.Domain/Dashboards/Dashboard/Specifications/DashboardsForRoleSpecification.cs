// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardsForRoleSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Concrete specification matching published Dashboards visible to a given
//                  viewer role, per "4. Role-Based Dashboards".
//                  Additive-only evolution.
// ============================================================================
using DashboardRole = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardRole;
using DashboardStatus = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardStatus;

namespace PlantSafety.Domain.Dashboards.Dashboard.Specifications;

/// <summary>Matches published <see cref="Aggregates.Dashboard"/> aggregates visible to <paramref name="viewerRole"/>.</summary>
public sealed class DashboardsForRoleSpecification(DashboardRole viewerRole) : DashboardSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.Dashboard, bool>> ToExpression()
        => dashboard => dashboard.Status == DashboardStatus.Published
                         && (dashboard.Role == viewerRole || viewerRole == DashboardRole.Executive);
}
