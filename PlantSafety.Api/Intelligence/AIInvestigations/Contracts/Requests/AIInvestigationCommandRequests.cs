// ============================================================================
// File: AIInvestigationCommandRequests.cs
// Project: PlantSafety.Api
// Module: Intelligence.AIInvestigations
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Intelligence.AIInvestigations
//              V1 controller's Phase 4 full build-out lifecycle actions (Create -> StartReview ->
//              Close -> Archive). Every mutating lifecycle request carries an optional ETag field
//              so clients can supply the optimistic-concurrency token captured from a prior read
//              (e.g. AIInvestigationDetailDto.ETag); a stale value results in a
//              concurrency-conflict error. Mirrors
//              PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Contracts.Requests
//              .RootCauseAnalysisCommandRequests.
// ============================================================================

namespace PlantSafety.Api.Intelligence.AIInvestigations.Contracts.Requests;

/// <summary>Request body for opening a new AI-assisted safety investigation.</summary>
public sealed record CreateAIInvestigationRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reference identifier of the related safety incident.</summary>
    public required string IncidentReference { get; init; }

    /// <summary>AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    public required double AiConfidenceScore { get; init; }

    /// <summary>AI-generated summary of the investigation.</summary>
    public string? Summary { get; init; }
}

/// <summary>Request body for starting review of an AIInvestigation (Open -&gt; InProgress).</summary>
public sealed record StartReviewAIInvestigationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this AIInvestigation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an AIInvestigation (InProgress -&gt; Closed).</summary>
public sealed record CloseAIInvestigationRequest
{
    /// <summary>Resolution summary for the investigation.</summary>
    public string? ResolutionSummary { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this AIInvestigation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed AIInvestigation (Closed -&gt; Archived).</summary>
public sealed record ArchiveAIInvestigationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this AIInvestigation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
