// ============================================================================
// File: ReportingRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Request DTOs bound from the query string for the
//              Analytics/Reporting Features/ controller.
// ============================================================================

namespace PlantSafety.Api.Features.Analytics.Reporting.Contracts.Requests;

/// <summary>Query-string bound parameters for generating a Safety Report.</summary>
public sealed record GetSafetyReportRequest
{
    /// <summary>Optional site filter. Null = tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (UTC, inclusive).</summary>
    public required DateTimeOffset FromUtc { get; init; }

    /// <summary>Reporting period end (UTC, inclusive).</summary>
    public required DateTimeOffset ToUtc { get; init; }

    /// <summary>Report type (e.g., Monthly, Quarterly, Annual, Custom).</summary>
    public required string ReportType { get; init; }
}
