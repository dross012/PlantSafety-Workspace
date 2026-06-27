// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Observation record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.Observation;

/// <summary>
/// Read-side projection of a Safety Observation.
/// An observation is a documented workplace safety condition — positive or requiring action.
/// </summary>
public sealed record ObservationDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid ObservationId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the observation was made.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Narrative description of the observed condition.</summary>
    public required string Description { get; init; }

    /// <summary>Type: Positive, AtRisk, Unsafe.</summary>
    public required string ObservationType { get; init; }

    /// <summary>UTC timestamp when the observation was made.</summary>
    public required DateTimeOffset ObservedAtUtc { get; init; }

    /// <summary>Worker who made the observation.</summary>
    public required Guid ObservedByWorkerId { get; init; }

    /// <summary>Workflow status (e.g., Open, ActionRequired, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
