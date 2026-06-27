// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Near Miss record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.NearMisses;

/// <summary>
/// Read-side projection of a NearMiss aggregate.
/// A near miss is an unplanned event that did not result in injury or damage but had the potential to do so.
/// </summary>
public sealed record NearMissDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid NearMissId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the near miss occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short descriptive title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Narrative of what happened and what was avoided.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the near miss occurred.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>Potential severity if the event had escalated.</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>Hazard category (e.g., Chemical, Mechanical, Ergonomic).</summary>
    public required string HazardCategory { get; init; }

    /// <summary>Current workflow status.</summary>
    public required string Status { get; init; }

    /// <summary>Worker who reported the near miss.</summary>
    public required Guid ReportedByWorkerId { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
