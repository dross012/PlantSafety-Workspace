// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for MachineGuardsBarriers aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Policies;

/// <summary>Determines whether a <see cref="MachineGuardsBarriers"/> aggregate is eligible for archival.</summary>
public static class MachineGuardsBarriersArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the guard record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers aggregate)
        => aggregate.Status == MachineGuardsBarriersStatus.Retired;
}
