// ============================================================================
// File: RCACommandRequests.cs
// Project: PlantSafety.Api
// Module: ContinuousImprovement.RCA
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the ContinuousImprovement.RCA
//              V1 controller's Phase 4 full build-out lifecycle actions
//              (Create -> StartAnalysis -> SubmitForApproval -> Close -> Archive). Mirrors
//              PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Contracts.Requests
//              .RootCauseAnalysisCommandRequests.cs -- every mutating lifecycle request carries an
//              optional ETag field so clients can supply the optimistic-concurrency token captured
//              from a prior read (e.g. RCADetailDto.ETag); a stale value results in a
//              concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.ContinuousImprovement.RCA.Contracts.Requests;

/// <summary>Request body for creating a new ContinuousImprovement RCA record.</summary>
public sealed record CreateRCARequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Problem statement being analyzed.</summary>
    public required string ProblemStatement { get; init; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public required string Methodology { get; init; }
}

/// <summary>Request body for beginning analysis work on an RCA record (Initiated -&gt; InProgress).</summary>
public sealed record StartAnalysisRCARequest
{
    /// <summary>Optimistic-concurrency token from the last read of this RCA record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for submitting an RCA record for approval (InProgress -&gt; PendingApproval).</summary>
public sealed record SubmitForApprovalRCARequest
{
    /// <summary>Findings recorded at submission time, if any.</summary>
    public string? Findings { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this RCA record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for approving and formally closing an RCA record (PendingApproval -&gt; Closed).</summary>
public sealed record CloseRCARequest
{
    /// <summary>Root cause summary recorded at closure time, if any.</summary>
    public string? RootCauseSummary { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this RCA record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed RCA record for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveRCARequest
{
    /// <summary>Optimistic-concurrency token from the last read of this RCA record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
