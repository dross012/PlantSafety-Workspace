// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlEquipmentType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of equipment types commonly fitted with two-hand control systems,
//                  per the PlantSafety Program Structure Two-Hand Controls component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Enums;

/// <summary>Classification of equipment types fitted with two-hand control systems.</summary>
public enum TwoHandControlEquipmentType
{
    /// <summary>Mechanical or hydraulic press.</summary>
    Press = 0,

    /// <summary>Cutting machine.</summary>
    Cutter = 1,

    /// <summary>Forming machine.</summary>
    FormingMachine = 2,
}
