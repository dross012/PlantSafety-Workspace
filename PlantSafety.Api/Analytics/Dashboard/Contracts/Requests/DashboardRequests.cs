// ============================================================================
// File: DashboardRequests.cs
// Project: PlantSafety.Api
// Module: Analytics.Dashboard
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Analytics.Dashboard V1
//              controller. Covers the full dashboard lifecycle: Create -> Publish -> RefreshData ->
//              Deprecate -> Close -> Archive. Every mutating lifecycle request carries an optional
//              ETag field so clients can supply the optimistic-concurrency token captured from a
//              prior read (e.g. DashboardItemDto.ETag); a stale value results in a
//              concurrency-conflict error. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================

namespace PlantSafety.Api.Analytics.Dashboard.Contracts.Requests;

/// <summary>Request body for defining a new KPI Dashboard.</summary>
public sealed record CreateDashboardRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable dashboard name.</summary>
    public required string DashboardName { get; init; }

    /// <summary>Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).</summary>
    public required string DashboardType { get; init; }

    /// <summary>Identity of the accountable dashboard owner. Defaults to the requesting user when omitted.</summary>
    public Guid? OwnerUserId { get; init; }

    /// <summary>Refresh interval, in seconds, for live data widgets. Defaults to 60.</summary>
    public int RefreshIntervalSeconds { get; init; } = 60;

    /// <summary>Count of widgets configured on this dashboard.</summary>
    public int WidgetCount { get; init; }
}

/// <summary>Request body for publishing a Dashboard for general use (Created -&gt; Active).</summary>
public sealed record PublishDashboardRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Dashboard record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording a data refresh for a published Dashboard. Does not change lifecycle status.</summary>
public sealed record RefreshDashboardDataRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Dashboard record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for deprecating a published Dashboard (Active -&gt; Completed).</summary>
public sealed record DeprecateDashboardRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Dashboard record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a deprecated Dashboard (Completed -&gt; Closed).</summary>
public sealed record CloseDashboardRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Dashboard record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Dashboard for long-term retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveDashboardRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Dashboard record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
