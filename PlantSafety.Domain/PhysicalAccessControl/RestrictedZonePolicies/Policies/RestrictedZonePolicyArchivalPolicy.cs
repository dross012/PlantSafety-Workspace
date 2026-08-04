// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for RestrictedZonePolicy
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Policies;

/// <summary>Determines whether a <see cref="RestrictedZonePolicy"/> aggregate is eligible for archival.</summary>
public static class RestrictedZonePolicyArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the policy record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy aggregate)
        => aggregate.Status == RestrictedZonePolicyStatus.Retired;
}
