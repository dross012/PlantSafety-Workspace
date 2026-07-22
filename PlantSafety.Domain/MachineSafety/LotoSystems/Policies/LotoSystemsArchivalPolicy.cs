// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for LotoSystems aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Policies;

/// <summary>Determines whether a <see cref="LotoSystems"/> aggregate is eligible for archival.</summary>
public static class LotoSystemsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the LOTO system record may be archived (released only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems aggregate)
        => aggregate.Status == LotoSystemsStatus.Released;
}
