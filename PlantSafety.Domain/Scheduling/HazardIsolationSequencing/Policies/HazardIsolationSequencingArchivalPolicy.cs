// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for HazardIsolationSequencing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Policies;

/// <summary>Determines whether a <see cref="HazardIsolationSequencing"/> aggregate is eligible for archival.</summary>
public static class HazardIsolationSequencingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the HazardIsolationSequencing may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate)
        => aggregate.Status == HazardIsolationSequencingStatus.Closed;
}