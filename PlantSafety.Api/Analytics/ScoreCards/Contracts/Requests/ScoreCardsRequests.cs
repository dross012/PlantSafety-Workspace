// ============================================================================
// File: ScoreCardsRequests.cs
// Project: PlantSafety.Api
// Module: Analytics.ScoreCards
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body/query string for the top-level
//              Analytics.ScoreCards V1 controller. Covers the full lifecycle:
//              Create -> Publish -> Finalize -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g. ScoreCardsItemDto.ETag);
//              a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Analytics.ScoreCards.Contracts.Requests;

/// <summary>Request body for creating a new ScoreCards record.</summary>
public sealed record CreateScoreCardsRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable scorecard name.</summary>
    public required string ScoreCardName { get; init; }

    /// <summary>Reporting period this scorecard covers (e.g., "2026-Q3").</summary>
    public required string ReportingPeriod { get; init; }

    /// <summary>KPI category this scorecard reports on (e.g., Safety, Compliance, Operations).</summary>
    public required string KpiCategory { get; init; }

    /// <summary>Computed performance index (0-100).</summary>
    public decimal PerformanceIndex { get; init; }

    /// <summary>Site this scorecard was generated for, if site-scoped. Null means enterprise-wide.</summary>
    public Guid? GeneratedForSiteId { get; init; }
}

/// <summary>Request body for publishing a ScoreCards record for consumption (Created -&gt; Active).</summary>
public sealed record PublishScoreCardsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ScoreCards record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for finalizing a published ScoreCards record (Active -&gt; Completed).</summary>
public sealed record FinalizeScoreCardsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ScoreCards record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a finalized ScoreCards record (Completed -&gt; Closed).</summary>
public sealed record CloseScoreCardsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ScoreCards record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed ScoreCards record for long-term retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveScoreCardsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ScoreCards record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the ScoreCards read-model list endpoint.</summary>
public sealed record ScoreCardsListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}
