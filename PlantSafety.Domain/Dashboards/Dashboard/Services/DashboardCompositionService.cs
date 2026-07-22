// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardCompositionService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Pure domain service resolving the effective grid bounds and overlap-free
//                  status of a Dashboard's widget layout, per "3. Dashboard Service Layer:
//                  Resolves which widgets belong to which dashboard" and "1. Architectural
//                  Layers ... Presentation Layer: Responsive layout: grid system for charts,
//                  tables, gauges, maps." Contains no infrastructure/rendering logic.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.Services;

/// <summary>Domain service resolving grid composition facts about a Dashboard's widget layout.</summary>
public sealed class DashboardCompositionService : IDomainService
{
    /// <summary>Returns the number of grid rows spanned by the dashboard's current widget layout.</summary>
    public int ResolveRowCount(Aggregates.Dashboard dashboard)
    {
        ArgumentNullException.ThrowIfNull(dashboard);
        return dashboard.WidgetLayout.Count == 0
            ? 0
            : dashboard.WidgetLayout.Max(p => p.Row + p.RowSpan);
    }

    /// <summary>
    /// Returns <see langword="true"/> when no two widget placements in the dashboard's layout
    /// overlap on the grid.
    /// </summary>
    public bool IsLayoutOverlapFree(Aggregates.Dashboard dashboard)
    {
        ArgumentNullException.ThrowIfNull(dashboard);

        var placements = dashboard.WidgetLayout;
        for (var i = 0; i < placements.Count; i++)
        {
            for (var j = i + 1; j < placements.Count; j++)
            {
                var a = placements[i];
                var b = placements[j];

                var rowsOverlap = a.Row < b.Row + b.RowSpan && b.Row < a.Row + a.RowSpan;
                var columnsOverlap = a.Column < b.Column + b.ColumnSpan && b.Column < a.Column + a.ColumnSpan;

                if (rowsOverlap && columnsOverlap)
                    return false;
            }
        }

        return true;
    }
}
