// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real inspection lifecycle states for the MachineGuarding aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Enums;

/// <summary>Lifecycle states for the <see cref="MachineGuarding"/> aggregate root.</summary>
public enum MachineGuardingStatus
{
    /// <summary>Guard has been installed and is awaiting its first inspection.</summary>
    Installed = 0,

    /// <summary>An inspection has been scheduled and is pending.</summary>
    InspectionScheduled = 1,

    /// <summary>Guard passed its most recent inspection with zero open deficiencies.</summary>
    Compliant = 2,

    /// <summary>Guard failed its most recent inspection; deficiencies are open.</summary>
    InspectionFailed = 3,

    /// <summary>Guard/interlock has been found bypassed, defeated, or removed.</summary>
    BypassDetected = 4,

    /// <summary>Corrective action is underway to remediate a failed inspection or bypass finding.</summary>
    CorrectiveActionInProgress = 5,

    /// <summary>Guard has been permanently retired (equipment decommissioned or guard replaced).</summary>
    Retired = 6,
}
