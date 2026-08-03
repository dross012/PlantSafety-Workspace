// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-model detail DTO for a single generated report, backing the
//   PlantSafety.UI Reporting module's Report Detail page.
//   Additive to the Analytics.Reporting module -- distinct from the pre-existing
//   ReportingItemDto/GetReportingQuery date-range surface, which is left unchanged.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Reporting.Models;

/// <summary>Full detail DTO for a single generated report.</summary>
public sealed record ReportDetailDto
{
    /// <summary>Report identifier.</summary>
    public required Guid ReportId { get; init; }

    /// <summary>Title of the report.</summary>
    public required string Title { get; init; }

    /// <summary>Description/summary of the report's contents.</summary>
    public required string Description { get; init; }

    /// <summary>Type/category of the report (e.g., Safety, Compliance, Operational, Financial).</summary>
    public required string ReportType { get; init; }

    /// <summary>Current status of the report (e.g., Draft, Generated, Published, Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Start date (inclusive) of the period this report covers.</summary>
    public required DateOnly PeriodStart { get; init; }

    /// <summary>End date (inclusive) of the period this report covers.</summary>
    public required DateOnly PeriodEnd { get; init; }

    /// <summary>UTC timestamp when the report was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Display name of the report owner.</summary>
    public required string OwnerName { get; init; }

    /// <summary>Export formats available for this report (e.g., PDF, CSV, XLSX).</summary>
    public required IReadOnlyList<string> AvailableFormats { get; init; }

    /// <summary>Name of the platform module this report was generated from.</summary>
    public required string SourceModule { get; init; }
}
