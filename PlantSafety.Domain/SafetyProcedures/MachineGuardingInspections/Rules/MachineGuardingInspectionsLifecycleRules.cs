// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the MachineGuardingInspections aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="MachineGuardingInspections"/> aggregate.</summary>
public static class MachineGuardingInspectionsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the inspection may be executed from <paramref name="current"/>.</summary>
    public static bool CanExecuteInspection(MachineGuardingInspectionsStatus current) => current == MachineGuardingInspectionsStatus.Scheduled;

    /// <summary>Returns <see langword="true"/> when corrective action may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginCorrectiveAction(MachineGuardingInspectionsStatus current) => current == MachineGuardingInspectionsStatus.DeficiencyLogged;

    /// <summary>Returns <see langword="true"/> when the inspection may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(MachineGuardingInspectionsStatus current) =>
        current is MachineGuardingInspectionsStatus.Passed or MachineGuardingInspectionsStatus.CorrectiveActionInProgress;
}
