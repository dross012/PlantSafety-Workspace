// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for CraneHoistSystems aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Policies;

/// <summary>Determines whether a <see cref="CraneHoistSystems"/> aggregate is eligible for archival.</summary>
public static class CraneHoistSystemsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the crane/hoist asset record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems aggregate)
        => aggregate.Status == CraneHoistSystemsStatus.Retired;
}
