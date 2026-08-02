// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           DrillsAndExercisesDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single DrillsAndExercises record, including
//                  every aggregate property plus an ETag. Backs GetDrillsAndExercisesByIdQuery.
//                  Distinct from the pre-existing, simpler DrillsAndExercisesItemDto used by the
//                  list-range query (GetDrillsAndExercisesQuery), which is left untouched. Adds
//                  ScheduledAtUtc, a legitimate additive write-model field the list DTO does not
//                  yet surface. Mirrors PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;

/// <summary>A single DrillsAndExercises read-model record.</summary>
public sealed record DrillsAndExercisesDetailDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DrillsAndExercisesId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Scheduled/Conducted/Completed/Closed/Archived/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>Type of drill (e.g., FireDrill/Evacuation/ShelterInPlace/SpillResponse).</summary>
    public required string DrillType { get; init; }

    /// <summary>Number of participants involved in the drill.</summary>
    public required int ParticipantCount { get; init; }

    /// <summary>UTC instant the drill was scheduled for. Null if never scheduled with a target time.</summary>
    public DateTime? ScheduledAtUtc { get; init; }

    /// <summary>UTC instant the drill was actually conducted.</summary>
    public DateTime? ConductedAtUtc { get; init; }

    /// <summary>Identity of the user who conducted the drill.</summary>
    public Guid? ConductedBy { get; init; }

    /// <summary>Identity of the user who completed the drill (recorded the debrief).</summary>
    public Guid? CompletedBy { get; init; }

    /// <summary>UTC instant the drill was completed.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Free-text debrief notes recorded on completion.</summary>
    public string? DebriefNotes { get; init; }

    /// <summary>Identity of the user who cancelled the drill.</summary>
    public Guid? CancelledBy { get; init; }

    /// <summary>Reason recorded when a scheduled drill is cancelled.</summary>
    public string? CancelledReason { get; init; }

    /// <summary>UTC instant the drill was cancelled.</summary>
    public DateTime? CancelledAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed the drill record.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>UTC instant the drill record was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the drill record was archived.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
