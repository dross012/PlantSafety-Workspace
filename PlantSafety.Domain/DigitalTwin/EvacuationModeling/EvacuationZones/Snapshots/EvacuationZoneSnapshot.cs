// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EvacuationZone aggregate rehydration. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.EvacuationZone"/> aggregate for fast rehydration.</summary>
public sealed record EvacuationZoneSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EvacuationZoneId EvacuationZoneId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EvacuationZoneStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
