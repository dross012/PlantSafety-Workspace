// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single dashboard widget row.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.ReadModel.Models;

/// <summary>A single row within the generic Dashboard read-model.</summary>
public sealed record ReadModelItemDto
{
    /// <summary>Row identifier.</summary>
    public required Guid RowId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the widget this row feeds.</summary>
    public required string WidgetName { get; init; }

    /// <summary>Rendered value for the widget row.</summary>
    public required string WidgetValue { get; init; }

    /// <summary>Ordinal position for display within the dashboard.</summary>
    public required int DisplayOrder { get; init; }

    /// <summary>Current status of the row (e.g., Active, Stale).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this row was last refreshed.</summary>
    public required DateTime RefreshedAtUtc { get; init; }
}
