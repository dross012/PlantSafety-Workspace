// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for TwoHandControls aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Policies;

/// <summary>Determines whether a <see cref="TwoHandControls"/> aggregate is eligible for archival.</summary>
public static class TwoHandControlsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the control record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls aggregate)
        => aggregate.Status == TwoHandControlsStatus.Retired;
}
