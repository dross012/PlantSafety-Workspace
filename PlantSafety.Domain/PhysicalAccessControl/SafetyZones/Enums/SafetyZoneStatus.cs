// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the SafetyZone aggregate root (a physical area whose
//                  access and occupancy are correlated from AccessEvent data).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Enums;

/// <summary>Lifecycle states for the <see cref="SafetyZone"/> aggregate root.</summary>
public enum SafetyZoneStatus
{
    /// <summary>Zone is active and open to holders of the required access level.</summary>
    Active = 0,

    /// <summary>Zone is under a restricted-access policy (permit-gated or elevated clearance required).</summary>
    Restricted = 1,

    /// <summary>Zone is inactive (decommissioned area, no occupancy tracked).</summary>
    Inactive = 2,
}
