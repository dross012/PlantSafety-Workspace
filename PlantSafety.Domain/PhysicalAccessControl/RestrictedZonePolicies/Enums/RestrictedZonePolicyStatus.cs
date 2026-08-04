// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the RestrictedZonePolicy aggregate root (defines which
//                  access levels/credentials may enter a given SafetyZone, and whether
//                  after-hours access is locked out).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Enums;

/// <summary>Lifecycle states for the <see cref="RestrictedZonePolicy"/> aggregate root.</summary>
public enum RestrictedZonePolicyStatus
{
    /// <summary>Policy has been defined but is not yet enforced.</summary>
    Draft = 0,

    /// <summary>Policy is active and enforced at its associated SafetyZone.</summary>
    Active = 1,

    /// <summary>Policy is temporarily suspended (not enforced).</summary>
    Suspended = 2,

    /// <summary>Policy has been permanently retired.</summary>
    Retired = 3,
}
