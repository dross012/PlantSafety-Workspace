// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for SafetyZone aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Policies;

/// <summary>Determines whether a <see cref="SafetyZone"/> aggregate is eligible for archival.</summary>
public static class SafetyZoneArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the zone record may be archived (inactive only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone aggregate)
        => aggregate.Status == SafetyZoneStatus.Inactive;
}
