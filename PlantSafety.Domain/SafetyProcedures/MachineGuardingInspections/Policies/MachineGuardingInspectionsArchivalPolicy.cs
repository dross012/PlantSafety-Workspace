// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for MachineGuardingInspections
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Policies;

/// <summary>Determines whether a <see cref="MachineGuardingInspections"/> aggregate is eligible for archival.</summary>
public static class MachineGuardingInspectionsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the inspection record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections aggregate)
        => aggregate.Status == MachineGuardingInspectionsStatus.Closed;
}
