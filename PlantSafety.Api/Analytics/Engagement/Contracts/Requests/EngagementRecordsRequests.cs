// ============================================================================
// File: EngagementRecordsRequests.cs
// Project: PlantSafety.Api
// Module: Analytics.Engagement
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Analytics.Engagement Records V1 controller. Covers the full Engagement lifecycle:
//              Create -> Review -> RecordTouchpoint -> Complete -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g. EngagementItemDto.ETag);
//              a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Contracts.Requests;

/// <summary>Request body for submitting a new Engagement measurement.</summary>
public sealed record CreateEngagementRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Worker this engagement measurement relates to.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Computed engagement score (0-100).</summary>
    public required decimal EngagementScore { get; init; }

    /// <summary>Survey period this measurement covers (e.g., "2026-Q3").</summary>
    public required string SurveyPeriod { get; init; }

    /// <summary>Channel through which engagement was captured (e.g., MobileApp, Kiosk, Paper, Interview).</summary>
    public required string ParticipationChannel { get; init; }

    /// <summary>Number of responses/interactions captured for this measurement.</summary>
    public int ResponseCount { get; init; }

    /// <summary>UTC timestamp this worker last engaged with a safety touchpoint, if known at creation time.</summary>
    public DateTime? LastEngagedAtUtc { get; init; }
}

/// <summary>Request body for reviewing and activating an Engagement measurement (Created -&gt; Active).</summary>
public sealed record ReviewEngagementRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Engagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording a worker touchpoint against an active Engagement measurement.</summary>
public sealed record RecordEngagementTouchpointRequest
{
    /// <summary>Updated response/interaction count for this measurement.</summary>
    public required int ResponseCount { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Engagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an active Engagement measurement (Active -&gt; Completed).</summary>
public sealed record CompleteEngagementRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Engagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed Engagement measurement (Completed -&gt; Closed).</summary>
public sealed record CloseEngagementRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Engagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Engagement measurement (Closed -&gt; Archived).</summary>
public sealed record ArchiveEngagementRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Engagement record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the Engagement read-model list endpoint.</summary>
public sealed record EngagementRecordsListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}
