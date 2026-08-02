// ============================================================================
// File: RootCauseAnalysisCommandRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.RootCauseAnalysis
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the
//              SafetyEvents.RootCauseAnalysis V1 controller's Phase 4 full build-out lifecycle
//              actions (Create -> SubmitForReview -> Approve -> Close -> Archive). Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              RootCauseAnalysisDetailDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Contracts.Requests;

/// <summary>Request body for starting a new RootCauseAnalysis engagement.</summary>
public sealed record CreateRootCauseAnalysisRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the related incident, near-miss, or other safety event, if any.</summary>
    public Guid? RelatedEventId { get; init; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public required string Methodology { get; init; }
}

/// <summary>Request body for submitting a RootCauseAnalysis for review (InProgress -&gt; PendingReview).</summary>
public sealed record SubmitForReviewRootCauseAnalysisRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this RootCauseAnalysis record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for reviewing and approving a RootCauseAnalysis (PendingReview -&gt; Completed).</summary>
public sealed record ApproveRootCauseAnalysisRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this RootCauseAnalysis record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed RootCauseAnalysis (Completed -&gt; Closed).</summary>
public sealed record CloseRootCauseAnalysisRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this RootCauseAnalysis record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed RootCauseAnalysis for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveRootCauseAnalysisRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this RootCauseAnalysis record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
