// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model DTO for a single Widget definition.
// SchemaVersion: 1

namespace PlantSafety.Application.Dashboards.Widget.Models;

/// <summary>Immutable read-model representation of a single Widget definition.</summary>
public sealed record WidgetDto
{
    /// <summary>Widget identifier.</summary>
    public required Guid WidgetId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Current lifecycle status (Draft/Active/Deprecated/Retired).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable widget name.</summary>
    public required string Name { get; init; }

    /// <summary>Widget visualization type code (e.g. "LineChart", "Gauge").</summary>
    public required string TypeCode { get; init; }

    /// <summary>Owning module the widget's data source belongs to.</summary>
    public required string SourceModule { get; init; }

    /// <summary>Query/endpoint identifier within the owning module.</summary>
    public required string QueryOrEndpoint { get; init; }

    /// <summary>Refresh cadence, in seconds.</summary>
    public required int RefreshIntervalSeconds { get; init; }

    /// <summary>Configuration parameters (key/value).</summary>
    public required IReadOnlyDictionary<string, string> Parameters { get; init; }

    /// <summary>Canonical role names permitted to view this widget. Empty means visible to all authorized viewers.</summary>
    public required IReadOnlyList<string> PermittedRoles { get; init; }
}
