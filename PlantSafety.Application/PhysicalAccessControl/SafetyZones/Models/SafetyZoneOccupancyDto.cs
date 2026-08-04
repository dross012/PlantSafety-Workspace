// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTO for a SafetyZone's occupancy, correlated from ingested
//   AccessEvent In/Out data. Backs the Live Access & Occupancy Map dashboard.
// SchemaVersion: 1

namespace PlantSafety.Application.PhysicalAccessControl.SafetyZones.Models;

/// <summary>Occupancy projection for a single SafetyZone, correlated from ingested AccessEvents.</summary>
public sealed record SafetyZoneOccupancyDto
{
    /// <summary>Record identifier.</summary>
    public required Guid SafetyZoneId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Zone code/label.</summary>
    public required string ZoneCode { get; init; }

    /// <summary>Zone name.</summary>
    public required string ZoneName { get; init; }

    /// <summary>Maximum permitted concurrent occupancy.</summary>
    public required int MaxOccupancy { get; init; }

    /// <summary>Current occupancy, as correlated from ingested AccessEvents.</summary>
    public required int CurrentOccupancy { get; init; }

    /// <summary>Overall zone status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp this occupancy figure was last recalculated.</summary>
    public required DateTime AsOfUtc { get; init; }
}
