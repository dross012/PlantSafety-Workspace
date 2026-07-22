// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real inspection lifecycle states for the MachineGuardingInspections aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Enums;

/// <summary>Lifecycle states for the <see cref="MachineGuardingInspections"/> aggregate root.</summary>
public enum MachineGuardingInspectionsStatus
{
    /// <summary>Inspection scheduled.</summary>
    Scheduled = 0,

    /// <summary>Inspection executed and passed with no deficiencies.</summary>
    Passed = 1,

    /// <summary>Inspection executed and logged one or more deficiencies.</summary>
    DeficiencyLogged = 2,

    /// <summary>Corrective action underway to remediate logged deficiencies.</summary>
    CorrectiveActionInProgress = 3,

    /// <summary>Corrective action verified; inspection closed out.</summary>
    Closed = 4,
}
