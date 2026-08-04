// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTOs for a single EvacuationZone record and its occupancy
//                    reading history ("DigitalTwin.EvacuationModeling.EvacuationZones").
// SchemaVersion: 1

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Models;

/// <summary>A single EvacuationZone record (an occupancy-tracked zone validated against the Digital Twin spatial model).</summary>
public sealed record EvacuationZoneItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EvacuationZoneId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Zone Name.</summary>
    public required string ZoneName { get; init; }

    /// <summary>Version of the Digital Twin model this zone's geometry was sourced from.</summary>
    public required string TwinModelVersion { get; init; }

    /// <summary>Rated maximum occupancy capacity of this zone.</summary>
    public required int ZoneCapacity { get; init; }

    /// <summary>Occupancy count above which an occupancy reading is flagged as exceeding threshold.</summary>
    public required int OccupancyThreshold { get; init; }

    /// <summary>Most recently recorded occupancy headcount estimate.</summary>
    public required int CurrentOccupancyCount { get; init; }

    /// <summary>UTC timestamp of the most recent occupancy reading, if any (feeds the occupancy-reading-freshness KPI).</summary>
    public DateTime? LastOccupancyReadingAtUtc { get; init; }

    /// <summary>Overall status classification: Active, OccupancyThresholdBreached, Retired.</summary>
    public required string ZoneStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}

/// <summary>A single aggregate/anonymized occupancy reading recorded against an EvacuationZone.</summary>
public sealed record OccupancyReadingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid OccupancyReadingId { get; init; }

    /// <summary>Identity of the parent EvacuationZone.</summary>
    public required Guid EvacuationZoneId { get; init; }

    /// <summary>Aggregate/anonymized headcount estimate.</summary>
    public required int HeadcountEstimate { get; init; }

    /// <summary>Source that produced this reading: BmsFeed, Sensor, Manual, SimulationDerived.</summary>
    public required string Source { get; init; }

    /// <summary>UTC timestamp the reading was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Whether this reading exceeded the zone's configured occupancy threshold.</summary>
    public required bool ExceededThreshold { get; init; }
}
