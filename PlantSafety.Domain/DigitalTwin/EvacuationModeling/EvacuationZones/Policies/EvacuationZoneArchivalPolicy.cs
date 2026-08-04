// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for EvacuationZone aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Policies;

/// <summary>Determines whether an <see cref="Aggregates.EvacuationZone"/> aggregate is eligible for archival.</summary>
public static class EvacuationZoneArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the zone record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(Aggregates.EvacuationZone aggregate)
        => aggregate.Status == EvacuationZoneStatus.Retired;
}
