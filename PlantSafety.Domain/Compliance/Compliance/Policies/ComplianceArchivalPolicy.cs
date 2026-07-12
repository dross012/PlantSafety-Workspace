// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Compliance aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Policies;

/// <summary>Determines whether a <see cref="Compliance"/> aggregate is eligible for archival.</summary>
public static class ComplianceArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Compliance may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance aggregate)
        => aggregate.Status == ComplianceStatus.Closed;
}
