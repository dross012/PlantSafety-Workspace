// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the RestrictedZonePolicy
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="RestrictedZonePolicy"/> aggregate.</summary>
public static class RestrictedZonePolicyLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the policy may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(RestrictedZonePolicyStatus current) =>
        current is RestrictedZonePolicyStatus.Draft or RestrictedZonePolicyStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when the policy may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(RestrictedZonePolicyStatus current) => current == RestrictedZonePolicyStatus.Active;

    /// <summary>Returns <see langword="true"/> when the policy may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(RestrictedZonePolicyStatus current) =>
        current is RestrictedZonePolicyStatus.Draft or RestrictedZonePolicyStatus.Active or RestrictedZonePolicyStatus.Suspended;
}
