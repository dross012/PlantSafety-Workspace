// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           ProcedureEnergySourceType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Flags enumeration of hazardous energy source types documented by a LOTO procedure,
//                  per the PlantSafety Program Structure LOTO (Safety Procedures) component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Enums;

/// <summary>Hazardous energy source types documented by a LOTO procedure.</summary>
[Flags]
public enum ProcedureEnergySourceType
{
    /// <summary>No energy source recorded.</summary>
    None = 0,

    /// <summary>Electrical energy.</summary>
    Electrical = 1 << 0,

    /// <summary>Hydraulic energy.</summary>
    Hydraulic = 1 << 1,

    /// <summary>Pneumatic energy.</summary>
    Pneumatic = 1 << 2,

    /// <summary>Thermal energy.</summary>
    Thermal = 1 << 3,

    /// <summary>Chemical energy.</summary>
    Chemical = 1 << 4,
}
