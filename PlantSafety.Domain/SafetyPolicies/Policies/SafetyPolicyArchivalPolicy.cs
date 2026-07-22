// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for SafetyPolicy aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPolicies.Aggregates;
using PlantSafety.Domain.SafetyPolicies.Enums;

namespace PlantSafety.Domain.SafetyPolicies.Policies;

/// <summary>Determines whether a SafetyPolicy aggregate is eligible for archival.</summary>
public static class SafetyPolicyArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the SafetyPolicy may be archived.</summary>
    public static bool IsEligibleForArchival(SafetyPolicy aggregate)
        => aggregate.Status == SafetyPolicyStatus.Retired;
}