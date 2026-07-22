// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model DTO for a single Widget placement within a Dashboard's layout.
// SchemaVersion: 1

namespace PlantSafety.Application.Dashboards.Dashboard.Models;

/// <summary>Immutable read-model representation of a single Widget placement.</summary>
public sealed record DashboardWidgetPlacementDto
{
    /// <summary>Placement identifier.</summary>
    public required Guid PlacementId { get; init; }

    /// <summary>Identifier of the placed Widget.</summary>
    public required Guid WidgetId { get; init; }

    /// <summary>Grid row position (0-based).</summary>
    public required int Row { get; init; }

    /// <summary>Grid column position (0-based).</summary>
    public required int Column { get; init; }

    /// <summary>Grid row span.</summary>
    public required int RowSpan { get; init; }

    /// <summary>Grid column span.</summary>
    public required int ColumnSpan { get; init; }

    /// <summary>Per-placement filter overrides applied on top of the dashboard's default filters.</summary>
    public required IReadOnlyDictionary<string, string> FilterOverrides { get; init; }
}
