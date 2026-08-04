// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           OccupancyReading.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single occupancy snapshot recorded against an
//                  EvacuationZone aggregate (Occupancy Tracking subdomain). Data-governance note:
//                  HeadcountEstimate is an aggregate/anonymized count only -- no individually
//                  identifiable occupant data is retained on this entity. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Entities;

/// <summary>Records a single occupancy snapshot on an <see cref="Aggregates.EvacuationZone"/> aggregate.</summary>
public sealed record OccupancyReading
{
    /// <summary>Identity of this occupancy reading.</summary>
    public OccupancyReadingId OccupancyReadingId { get; }

    /// <summary>Identity of the parent EvacuationZone aggregate.</summary>
    public EvacuationZoneId EvacuationZoneId { get; }

    /// <summary>Aggregate/anonymized headcount estimate for the zone at the time of reading.</summary>
    public int HeadcountEstimate { get; }

    /// <summary>Source that produced this reading.</summary>
    public OccupancyReadingSource Source { get; }

    /// <summary>UTC timestamp the reading was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; }

    /// <summary>Whether this reading exceeded the zone's configured occupancy threshold at recording time.</summary>
    public bool ExceededThreshold { get; }

    /// <summary>Creates a new occupancy reading.</summary>
    public OccupancyReading(
        OccupancyReadingId occupancyReadingId, EvacuationZoneId evacuationZoneId,
        int headcountEstimate, OccupancyReadingSource source,
        UtcTimestamp recordedAtUtc, bool exceededThreshold)
    {
        OccupancyReadingId = occupancyReadingId;
        EvacuationZoneId   = evacuationZoneId;
        HeadcountEstimate  = headcountEstimate;
        Source             = source;
        RecordedAtUtc      = recordedAtUtc;
        ExceededThreshold  = exceededThreshold;
    }
}
