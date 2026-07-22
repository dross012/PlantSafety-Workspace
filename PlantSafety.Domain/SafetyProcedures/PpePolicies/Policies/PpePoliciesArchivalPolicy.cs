// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for PpePolicies aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Policies;

/// <summary>Determines whether a <see cref="PpePolicies"/> aggregate is eligible for archival.</summary>
public static class PpePoliciesArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the policy document may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies aggregate)
        => aggregate.Status == PpePoliciesStatus.Retired;
}
