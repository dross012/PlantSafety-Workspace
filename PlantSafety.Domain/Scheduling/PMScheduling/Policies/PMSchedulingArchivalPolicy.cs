// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PMScheduling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Policies;

/// <summary>Determines whether a <see cref="PMScheduling"/> aggregate is eligible for archival.</summary>
public static class PMSchedulingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PMScheduling may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate)
        => aggregate.Status == PMSchedulingStatus.Closed;
}