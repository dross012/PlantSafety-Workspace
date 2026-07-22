// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model DTO for a single Dashboard.
// SchemaVersion: 1

namespace PlantSafety.Application.Dashboards.Dashboard.Models;

/// <summary>Immutable read-model representation of a single Dashboard.</summary>
public sealed record DashboardDto
{
    /// <summary>Dashboard identifier.</summary>
    public required Guid DashboardId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/Published/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable dashboard name.</summary>
    public required string Name { get; init; }

    /// <summary>Narrative description.</summary>
    public required string Description { get; init; }

    /// <summary>Catalog category code (e.g. "ExecutiveSafetyOverview").</summary>
    public required string CategoryCode { get; init; }

    /// <summary>Primary owning role (e.g. "Executive", "SafetyManager").</summary>
    public required string Role { get; init; }

    /// <summary>Default filters applied when the dashboard is opened.</summary>
    public required IReadOnlyDictionary<string, string> DefaultFilters { get; init; }

    /// <summary>Widget placements composing this dashboard's layout.</summary>
    public required IReadOnlyList<DashboardWidgetPlacementDto> WidgetLayout { get; init; }
}
