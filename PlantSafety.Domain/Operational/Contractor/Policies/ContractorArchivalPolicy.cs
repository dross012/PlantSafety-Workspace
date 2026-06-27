// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Contractor aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Policies;

/// <summary>Determines whether a <see cref="Contractor"/> aggregate is eligible for archival.</summary>
public static class ContractorArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Contractor may be archived.</summary>
    public static bool IsEligibleForArchival(Contractor aggregate)
        => aggregate.Status == ContractorStatus.Closed;
}
