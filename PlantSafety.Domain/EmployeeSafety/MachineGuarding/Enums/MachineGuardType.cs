// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of machine guard control types per the PlantSafety Program Structure
//                  "Machine Guarding" component: fixed guards, interlocked doors, adjustable guards, and
//                  presence-sensing devices.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Enums;

/// <summary>Classification of machine guard control types.</summary>
public enum MachineGuardType
{
    /// <summary>Fixed guard/barrier that permanently blocks access to hazardous moving parts.</summary>
    FixedGuard = 0,

    /// <summary>Interlocked door that disables machine operation when opened.</summary>
    InterlockedDoor = 1,

    /// <summary>Adjustable guard for tools or cutting equipment requiring operator positioning.</summary>
    AdjustableGuard = 2,

    /// <summary>Presence-sensing device (light curtain, laser scanner, or pressure mat) integrated with the guard.</summary>
    PresenceSensingDevice = 3,
}
