// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardWidgetPlacement.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Entity representing a single Widget's placement within a Dashboard's
//                  WidgetLayout, per "1. Dashboard: Has: ... WidgetLayout" and
//                  "6. Extensibility: Dashboard definitions stored as config (JSON/YAML/db) so
//                  you can: Add/remove widgets, Change layouts". References the Widget aggregate
//                  by StrongId only (cross-aggregate references use StrongId only per DDD
//                  Aggregate Rules).
//                  Additive-only evolution.
// ============================================================================
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardFilter = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.DashboardFilter;

namespace PlantSafety.Domain.Dashboards.Dashboard.Entities;

/// <summary>Represents a single Widget's placement within a <see cref="Aggregates.Dashboard"/>'s layout.</summary>
public sealed class DashboardWidgetPlacement
{
    /// <summary>Unique identity of this placement record.</summary>
    public Guid PlacementId { get; private set; }

    /// <summary>Identity of the parent Dashboard aggregate.</summary>
    public DashboardId DashboardId { get; private set; }

    /// <summary>Identity of the placed Widget aggregate (StrongId reference only; no direct object reference).</summary>
    public PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId WidgetId { get; private set; }

    /// <summary>Grid row position (0-based).</summary>
    public int Row { get; private set; }

    /// <summary>Grid column position (0-based).</summary>
    public int Column { get; private set; }

    /// <summary>Grid row span.</summary>
    public int RowSpan { get; private set; }

    /// <summary>Grid column span.</summary>
    public int ColumnSpan { get; private set; }

    /// <summary>Optional per-placement filter overrides applied on top of the dashboard's default filters.</summary>
    public IReadOnlyList<DashboardFilter> FilterOverrides { get; private set; } = [];

    /// <summary>For rehydration only.</summary>
    private DashboardWidgetPlacement() { }

    /// <summary>Creates a new widget placement.</summary>
    public DashboardWidgetPlacement(
        Guid placementId,
        DashboardId dashboardId,
        PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId widgetId,
        int row,
        int column,
        int rowSpan = 1,
        int columnSpan = 1,
        IReadOnlyList<DashboardFilter>? filterOverrides = null)
    {
        if (row < 0) throw new ArgumentOutOfRangeException(nameof(row), "Row must be non-negative.");
        if (column < 0) throw new ArgumentOutOfRangeException(nameof(column), "Column must be non-negative.");
        if (rowSpan < 1) throw new ArgumentOutOfRangeException(nameof(rowSpan), "RowSpan must be at least 1.");
        if (columnSpan < 1) throw new ArgumentOutOfRangeException(nameof(columnSpan), "ColumnSpan must be at least 1.");

        PlacementId     = placementId;
        DashboardId     = dashboardId;
        WidgetId        = widgetId;
        Row             = row;
        Column          = column;
        RowSpan         = rowSpan;
        ColumnSpan      = columnSpan;
        FilterOverrides = filterOverrides ?? [];
    }
}
