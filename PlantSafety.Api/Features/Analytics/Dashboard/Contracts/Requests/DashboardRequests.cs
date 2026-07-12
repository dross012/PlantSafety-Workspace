// ============================================================================
// File: DashboardRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: Request DTOs bound from the query string for the
//              Analytics/Dashboard Features/ controller.
// ============================================================================

namespace PlantSafety.Api.Features.Analytics.Dashboard.Contracts.Requests;

/// <summary>Query-string bound filters for the Safety Dashboard KPI snapshot.</summary>
public sealed record GetSafetyDashboardRequest
{
    /// <summary>Optional site filter. Null returns tenant-wide roll-up.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the reporting window (defaults to 30 days ago).</summary>
    public DateTimeOffset? WindowFromUtc { get; init; }

    /// <summary>Inclusive end of the reporting window (defaults to now).</summary>
    public DateTimeOffset? WindowToUtc { get; init; }
}
