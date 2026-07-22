// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for SafetyValvesRelays aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Policies;

/// <summary>Determines whether a <see cref="SafetyValvesRelays"/> aggregate is eligible for archival.</summary>
public static class SafetyValvesRelaysArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the component record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays aggregate)
        => aggregate.Status == SafetyValvesRelaysStatus.Retired;
}
