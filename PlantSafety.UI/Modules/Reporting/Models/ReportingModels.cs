// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for PlantSafety's general-purpose
//   Reporting module (distinct from the separate Reporting.UI app). Covers
//   the Reports List and Report Detail pages using the generic list/detail
//   template (no dedicated architecture manual for this module).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Reporting.Models;

/// <summary>Lightweight list-view DTO for a generated report.</summary>
public sealed class ReportListItemDto
{
    /// <summary>Gets the report identifier (UUIDv7).</summary>
    public Guid ReportId { get; init; }

    /// <summary>Gets the report title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the report type/category (e.g. Safety, Compliance, Operational, Financial).</summary>
    public string ReportType { get; init; } = string.Empty;

    /// <summary>Gets the report status (Draft, Generated, Published, Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the report was generated.</summary>
    public DateTimeOffset GeneratedAtUtc { get; init; }

    /// <summary>Gets the display name of the report owner.</summary>
    public string OwnerName { get; init; } = string.Empty;
}

/// <summary>Full report detail DTO.</summary>
public sealed class ReportDetailDto
{
    /// <summary>Gets the report identifier.</summary>
    public Guid ReportId { get; init; }

    /// <summary>Gets the report title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the report description/summary.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the report type/category.</summary>
    public string ReportType { get; init; } = string.Empty;

    /// <summary>Gets the report status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the reporting period start date.</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>Gets the reporting period end date.</summary>
    public DateOnly PeriodEnd { get; init; }

    /// <summary>Gets the UTC timestamp the report was generated.</summary>
    public DateTimeOffset GeneratedAtUtc { get; init; }

    /// <summary>Gets the display name of the report owner.</summary>
    public string OwnerName { get; init; } = string.Empty;

    /// <summary>Gets the available export formats (PDF, CSV, XLSX).</summary>
    public IReadOnlyList<string> AvailableFormats { get; init; } = [];

    /// <summary>Gets the source module this report was generated from.</summary>
    public string SourceModule { get; init; } = string.Empty;
}
