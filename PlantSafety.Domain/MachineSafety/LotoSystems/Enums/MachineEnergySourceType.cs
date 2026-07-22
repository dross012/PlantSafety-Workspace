// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           MachineEnergySourceType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Flags enumeration of hazardous energy source types isolated by a machine-level LOTO
//                  system, per the PlantSafety Program Structure LOTO Systems component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Enums;

/// <summary>Hazardous energy source types isolated by a machine-level LOTO system.</summary>
[Flags]
public enum MachineEnergySourceType
{
    /// <summary>No energy source recorded.</summary>
    None = 0,

    /// <summary>Electrical energy (switches, breakers).</summary>
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
