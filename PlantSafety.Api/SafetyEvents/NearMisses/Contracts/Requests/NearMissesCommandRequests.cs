// ============================================================================
// File: NearMissesCommandRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.NearMisses
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the SafetyEvents.NearMisses V1
//              controller's Phase 4 full build-out lifecycle actions (Create -> StartReview ->
//              Complete -> Close -> Archive). Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              NearMissesDetailDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.NearMisses.Contracts.Requests;

/// <summary>Request body for reporting a new NearMisses record.</summary>
public sealed record CreateNearMissesRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Hazard category (e.g., Slip/Trip/Fall, EquipmentFailure, ChemicalExposure).</summary>
    public required string Category { get; init; }

    /// <summary>Potential severity had the event resulted in an actual incident.</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>UTC instant the near-miss was reported. Defaults to now when not supplied.</summary>
    public DateTime? ReportedAtUtc { get; init; }
}

/// <summary>Request body for starting review of a NearMisses record (Reported -&gt; UnderReview).</summary>
public sealed record StartReviewNearMissesRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this NearMisses record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing review of a NearMisses record (UnderReview -&gt; Completed).</summary>
public sealed record CompleteNearMissesRequest
{
    /// <summary>Free-text findings recorded when the review is completed.</summary>
    public string? Findings { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this NearMisses record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed NearMisses record (Completed -&gt; Closed).</summary>
public sealed record CloseNearMissesRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this NearMisses record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed NearMisses record for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveNearMissesRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this NearMisses record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
