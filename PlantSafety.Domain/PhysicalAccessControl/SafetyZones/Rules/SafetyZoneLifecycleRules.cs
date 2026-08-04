// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the SafetyZone aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="SafetyZone"/> aggregate.</summary>
public static class SafetyZoneLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the zone may be restricted from <paramref name="current"/>.</summary>
    public static bool CanRestrict(SafetyZoneStatus current) => current == SafetyZoneStatus.Active;

    /// <summary>Returns <see langword="true"/> when the zone may be deactivated from <paramref name="current"/>.</summary>
    public static bool CanDeactivate(SafetyZoneStatus current) =>
        current is SafetyZoneStatus.Active or SafetyZoneStatus.Restricted;

    /// <summary>Returns <see langword="true"/> when occupancy may be updated for a zone in <paramref name="current"/>.</summary>
    public static bool CanUpdateOccupancy(SafetyZoneStatus current) =>
        current is SafetyZoneStatus.Active or SafetyZoneStatus.Restricted;
}
