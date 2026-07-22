// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of machine guarding inspection checks per the PlantSafety Program
//                  Structure Machine Guarding Inspections component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Enums;

/// <summary>Classification of machine guarding inspection checks.</summary>
public enum MachineGuardingInspectionCategory
{
    /// <summary>Verifies guards are securely mounted and undamaged.</summary>
    GuardMountingCheck = 0,

    /// <summary>Tests interlocks and presence-sensing devices.</summary>
    InterlockTest = 1,

    /// <summary>Checks for bypassed or removed guards.</summary>
    BypassCheck = 2,
}
