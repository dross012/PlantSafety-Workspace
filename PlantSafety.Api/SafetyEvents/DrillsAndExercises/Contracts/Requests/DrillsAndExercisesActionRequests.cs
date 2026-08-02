// ============================================================================
// File: DrillsAndExercisesActionRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.DrillsAndExercises
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the SafetyEvents.DrillsAndExercises
//              Actions V1 controller. Covers the full DrillsAndExercises lifecycle: Schedule ->
//              Conduct -> Complete / Cancel -> Close -> Archive. Mirrors
//              PlantSafety.Api.Operational.PredictiveMaintenance.Contracts.Requests
//              .PredictiveMaintenanceAlertRequests.cs -- every mutating lifecycle request carries
//              an optional ETag field so clients can supply the optimistic-concurrency token
//              captured from a prior read (e.g. DrillsAndExercisesDetailDto.ETag); a stale value
//              results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Contracts.Requests;

/// <summary>Request body for scheduling a new DrillsAndExercises drill.</summary>
public sealed record ScheduleDrillsAndExercisesRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type of drill (e.g., FireDrill/Evacuation/ShelterInPlace/SpillResponse).</summary>
    public required string DrillType { get; init; }

    /// <summary>Optional UTC instant the drill is scheduled for.</summary>
    public DateTime? ScheduledAtUtc { get; init; }
}

/// <summary>Request body for recording that a scheduled drill was conducted (Scheduled -&gt; Conducted).</summary>
public sealed record ConductDrillsAndExercisesRequest
{
    /// <summary>Number of participants involved in the drill.</summary>
    public required int ParticipantCount { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this drill. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing a conducted drill, recording the debrief (Conducted -&gt; Completed).</summary>
public sealed record CompleteDrillsAndExercisesRequest
{
    /// <summary>Free-text debrief notes.</summary>
    public string? DebriefNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this drill. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for cancelling a scheduled drill before it is conducted (Scheduled -&gt; Cancelled).</summary>
public sealed record CancelDrillsAndExercisesRequest
{
    /// <summary>Reason for cancellation.</summary>
    public required string Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this drill. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for lifecycle transitions that only require an optional concurrency token.</summary>
public sealed record DrillsAndExercisesLifecycleRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this drill. Null skips the check.</summary>
    public string? ETag { get; init; }
}
