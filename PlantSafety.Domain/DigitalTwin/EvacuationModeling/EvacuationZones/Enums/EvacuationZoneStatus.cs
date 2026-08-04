// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Real lifecycle states for the EvacuationZone aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.EvacuationZone"/> aggregate root.</summary>
public enum EvacuationZoneStatus
{
    /// <summary>Zone defined and being actively occupancy-tracked.</summary>
    Active = 0,

    /// <summary>Zone's most recent occupancy reading breached its configured threshold.</summary>
    OccupancyThresholdBreached = 1,

    /// <summary>Zone permanently retired/decommissioned.</summary>
    Retired = 2,
}
