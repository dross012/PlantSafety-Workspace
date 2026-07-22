// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for MachineGuarding aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Policies;

/// <summary>Determines whether a <see cref="MachineGuarding"/> aggregate is eligible for archival.</summary>
public static class MachineGuardingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the MachineGuarding record may be archived (retired guards only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding aggregate)
        => aggregate.Status == MachineGuardingStatus.Retired;
}
