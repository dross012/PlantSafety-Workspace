// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the MachineGuarding aggregate's real
//                  inspection lifecycle.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="MachineGuarding"/> aggregate.</summary>
public static class MachineGuardingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an inspection may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleInspection(MachineGuardingStatus current) =>
        current is MachineGuardingStatus.Installed or MachineGuardingStatus.Compliant or MachineGuardingStatus.InspectionFailed;

    /// <summary>Returns <see langword="true"/> when an inspection result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordInspectionResult(MachineGuardingStatus current) => current == MachineGuardingStatus.InspectionScheduled;

    /// <summary>Returns <see langword="true"/> when a bypass may be flagged from <paramref name="current"/>.</summary>
    public static bool CanFlagBypass(MachineGuardingStatus current) => current != MachineGuardingStatus.Retired;

    /// <summary>Returns <see langword="true"/> when corrective action may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleCorrectiveAction(MachineGuardingStatus current) =>
        current is MachineGuardingStatus.InspectionFailed or MachineGuardingStatus.BypassDetected;

    /// <summary>Returns <see langword="true"/> when corrective action may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerifyCorrectiveAction(MachineGuardingStatus current) => current == MachineGuardingStatus.CorrectiveActionInProgress;

    /// <summary>Returns <see langword="true"/> when the guard record may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(MachineGuardingStatus current) => current != MachineGuardingStatus.Retired;
}
