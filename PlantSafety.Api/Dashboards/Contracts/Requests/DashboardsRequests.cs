// ============================================================================
// File: DashboardsRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Dashboards V1 controller (Dashboard aggregate).
// ============================================================================

namespace PlantSafety.Api.Dashboards.Contracts.Requests;

/// <summary>Request body for creating a new draft Dashboard.</summary>
public sealed record CreateDashboardRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable dashboard name.</summary>
    public required string Name { get; init; }

    /// <summary>Narrative description.</summary>
    public required string Description { get; init; }

    /// <summary>Catalog category code (one of the 19 named program dashboards).</summary>
    public required string CategoryCode { get; init; }

    /// <summary>Default filters applied when the dashboard is opened (plant, line, department, shift).</summary>
    public Dictionary<string, string>? DefaultFilters { get; init; }
}

/// <summary>Request body for adding a Widget placement to a Dashboard's layout.</summary>
public sealed record AddWidgetToDashboardRequest
{
    /// <summary>Identifier of the widget to place.</summary>
    public required Guid WidgetId { get; init; }

    /// <summary>Grid row position (0-based).</summary>
    public required int Row { get; init; }

    /// <summary>Grid column position (0-based).</summary>
    public required int Column { get; init; }

    /// <summary>Grid row span.</summary>
    public required int RowSpan { get; init; }

    /// <summary>Grid column span.</summary>
    public required int ColumnSpan { get; init; }

    /// <summary>Optional per-placement filter overrides.</summary>
    public Dictionary<string, string>? FilterOverrides { get; init; }
}
