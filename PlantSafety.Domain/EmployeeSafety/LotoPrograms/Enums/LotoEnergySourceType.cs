// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoEnergySourceType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Flags enumeration of hazardous energy source types that a LOTO program must identify
//                  and isolate, per the PlantSafety Program Structure LOTO component. A single program
//                  may involve more than one energy source (e.g. electrical + hydraulic).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Enums;

/// <summary>Hazardous energy source types requiring isolation under a LOTO program.</summary>
[Flags]
public enum LotoEnergySourceType
{
    /// <summary>No energy source recorded.</summary>
    None = 0,

    /// <summary>Electrical energy (breakers, disconnects, switches).</summary>
    Electrical = 1 << 0,

    /// <summary>Hydraulic energy (pressurized fluid systems).</summary>
    Hydraulic = 1 << 1,

    /// <summary>Pneumatic energy (compressed air systems).</summary>
    Pneumatic = 1 << 2,

    /// <summary>Thermal energy (heated surfaces, steam).</summary>
    Thermal = 1 << 3,

    /// <summary>Chemical energy (stored/residual chemical hazards).</summary>
    Chemical = 1 << 4,
}
