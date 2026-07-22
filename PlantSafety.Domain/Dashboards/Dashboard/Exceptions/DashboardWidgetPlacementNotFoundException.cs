// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardWidgetPlacementNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when attempting to remove a Widget placement that does not exist on a
//                  Dashboard's layout.
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;

namespace PlantSafety.Domain.Dashboards.Dashboard.Exceptions;

/// <summary>Thrown when a Widget placement lookup on a <see cref="Aggregates.Dashboard"/>'s layout finds no match.</summary>
public sealed class DashboardWidgetPlacementNotFoundException : DashboardDomainException
{
    /// <summary>Creates a not-found exception for the given Dashboard/Widget pair.</summary>
    public DashboardWidgetPlacementNotFoundException(DashboardId dashboardId, Guid widgetId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Widget '{widgetId}' is not placed on Dashboard '{dashboardId}'.",
            "Dashboard.WidgetPlacementNotFound",
            "NotFound",
            "Low",
            correlationId,
            causationId)
    {
    }
}
