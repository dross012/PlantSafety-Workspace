// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single drill or exercise record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;

/// <summary>A single drill or emergency exercise conducted at a site.</summary>
public sealed record DrillsAndExercisesItemDto
{
    /// <summary>Drill/exercise identifier.</summary>
    public required Guid DrillId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type of drill (e.g., FireDrill/Evacuation/ShelterInPlace/SpillResponse).</summary>
    public required string DrillType { get; init; }

    /// <summary>Current drill status (e.g., Scheduled/Conducted/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the drill was conducted.</summary>
    public required DateTime ConductedAtUtc { get; init; }

    /// <summary>Identity of the user who conducted the drill.</summary>
    public required Guid ConductedBy { get; init; }

    /// <summary>Number of participants involved in the drill.</summary>
    public required int ParticipantCount { get; init; }
}
