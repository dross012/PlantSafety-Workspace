// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for ManagementOfChange aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Policies;

/// <summary>Determines whether a <see cref="ManagementOfChange"/> aggregate is eligible for archival.</summary>
public static class ManagementOfChangeArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the ManagementOfChange may be archived.</summary>
    public static bool IsEligibleForArchival(ManagementOfChange aggregate)
        => aggregate.Status == ManagementOfChangeStatus.Closed;
}
