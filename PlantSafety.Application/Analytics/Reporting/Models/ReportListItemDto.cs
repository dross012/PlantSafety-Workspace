// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-model list-item DTO for the paginated/type-filtered report catalog
//   surface backing the PlantSafety.UI Reporting module's Reports List page.
//   Additive to the Analytics.Reporting module -- distinct from the pre-existing
//   ReportingItemDto/GetReportingQuery date-range surface, which is left unchanged.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Reporting.Models;

/// <summary>Lightweight list-view DTO for a single generated report in the report catalog.</summary>
public sealed record ReportListItemDto
{
    /// <summary>Report identifier.</summary>
    public required Guid ReportId { get; init; }

    /// <summary>Title of the report.</summary>
    public required string Title { get; init; }

    /// <summary>Type/category of the report (e.g., Safety, Compliance, Operational, Financial).</summary>
    public required string ReportType { get; init; }

    /// <summary>Current status of the report (e.g., Draft, Generated, Published, Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the report was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Display name of the report owner.</summary>
    public required string OwnerName { get; init; }
}
