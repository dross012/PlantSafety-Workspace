// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Drill / Exercise record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.DrillsAndExercises;

/// <summary>
/// Read-side projection of a Safety Drill aggregate.
/// Drills are scheduled or ad-hoc exercises to test emergency response readiness.
/// </summary>
public sealed record DrillDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid DrillId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the drill was conducted.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Description of objectives and scope.</summary>
    public required string Description { get; init; }

    /// <summary>Drill type (e.g., Evacuation, FireResponse, ChemicalSpill, MedicalEmergency).</summary>
    public required string DrillType { get; init; }

    /// <summary>UTC timestamp when the drill was scheduled.</summary>
    public required DateTimeOffset ScheduledAtUtc { get; init; }

    /// <summary>UTC timestamp when the drill actually occurred, if executed.</summary>
    public DateTimeOffset? ExecutedAtUtc { get; init; }

    /// <summary>Workflow status (e.g., Scheduled, Completed, Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>Drill coordinator.</summary>
    public required Guid CoordinatorId { get; init; }

    /// <summary>Overall score or outcome rating, if evaluated.</summary>
    public int? OutcomeScore { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
