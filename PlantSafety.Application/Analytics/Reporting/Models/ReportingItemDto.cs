// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single generated analytics report.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Reporting.Models;

/// <summary>A single generated report entry within the Analytics reporting read-model.</summary>
public sealed record ReportingItemDto
{
    /// <summary>Report item identifier.</summary>
    public required Guid ReportItemId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the report.</summary>
    public required string ReportName { get; init; }

    /// <summary>Category of the report (e.g., Compliance, Safety, Operational).</summary>
    public required string ReportCategory { get; init; }

    /// <summary>Current status of the report (e.g., Queued, Generated, Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the report was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Identity of the user or process that generated the report.</summary>
    public required Guid GeneratedBy { get; init; }
}
