// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Policy aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Policies;

/// <summary>Determines whether a <see cref="Policy"/> aggregate is eligible for archival.</summary>
public static class PolicyArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Policy may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy aggregate)
        => aggregate.Status == PolicyStatus.Closed;
}
