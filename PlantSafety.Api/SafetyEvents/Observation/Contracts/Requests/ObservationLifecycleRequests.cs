// ============================================================================
// File: ObservationLifecycleRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.Observation
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the SafetyEvents.Observation
//              V1 controller's write endpoints. Covers the full lifecycle: Record -> Activate ->
//              Complete -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g. ObservationItemDto.ETag);
//              a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.Observation.Contracts.Requests;

/// <summary>Request body for recording a new field observation.</summary>
public sealed record RecordObservationRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Observation classification: SafeBehavior, AtRiskBehavior, HazardCondition.</summary>
    public string ObservationType { get; init; } = "SafeBehavior";

    /// <summary>Hazard category, when the observation identifies a hazard (e.g., Slip/Trip, Ergonomic, PPE).</summary>
    public string HazardCategory { get; init; } = string.Empty;

    /// <summary>Physical location the observation was made at.</summary>
    public required string Location { get; init; }

    /// <summary>Narrative description of what was observed.</summary>
    public required string Description { get; init; }

    /// <summary>Indicates whether corrective action is required as a result of this observation.</summary>
    public bool CorrectiveActionRequired { get; init; }

    /// <summary>UTC timestamp any required follow-up is due.</summary>
    public DateTime? FollowUpDueAtUtc { get; init; }
}

/// <summary>Request body for beginning follow-up tracking for an observation (Created -&gt; Active).</summary>
public sealed record ActivateObservationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Observation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording completion of the required follow-up/corrective action (Active -&gt; Completed).</summary>
public sealed record CompleteObservationRequest
{
    /// <summary>Optional free-text notes describing how the corrective action was resolved.</summary>
    public string? ResolutionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this Observation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a completed observation record (Completed -&gt; Closed).</summary>
public sealed record CloseObservationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Observation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed observation record for retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveObservationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Observation record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
