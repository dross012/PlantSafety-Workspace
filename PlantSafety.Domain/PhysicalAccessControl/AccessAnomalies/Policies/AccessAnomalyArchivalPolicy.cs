// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AccessAnomaly aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Policies;

/// <summary>Determines whether an <see cref="AccessAnomaly"/> aggregate is eligible for archival.</summary>
public static class AccessAnomalyArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the anomaly record may be archived (resolved only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly aggregate)
        => aggregate.Status == AccessAnomalyStatus.Resolved;
}
