// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single near-miss record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.NearMisses.Models;

/// <summary>A single near-miss event tracked by the NearMisses module.</summary>
public sealed record NearMissesItemDto
{
    /// <summary>Near-miss identifier.</summary>
    public required Guid NearMissId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Hazard category (e.g., Slip/Trip/Fall, EquipmentFailure, ChemicalExposure).</summary>
    public required string Category { get; init; }

    /// <summary>Potential severity had the event resulted in an actual incident (e.g., Low/Medium/High/Critical).</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>Current near-miss status (e.g., Reported/UnderReview/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the near-miss was reported.</summary>
    public required DateTime ReportedAtUtc { get; init; }

    /// <summary>Identity of the user who reported the near-miss.</summary>
    public required Guid ReportedBy { get; init; }
}
