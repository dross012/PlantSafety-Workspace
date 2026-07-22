// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for ShiftFatigue aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Policies;

/// <summary>Determines whether a <see cref="ShiftFatigue"/> aggregate is eligible for archival.</summary>
public static class ShiftFatigueArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the ShiftFatigue may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue aggregate)
        => aggregate.Status == ShiftFatigueStatus.Closed;
}