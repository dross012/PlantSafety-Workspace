// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicHazardCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of ergonomic hazard categories per the PlantSafety Program Structure
//                  Ergonomics & Material Handling component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Enums;

/// <summary>Classification of ergonomic hazard categories.</summary>
public enum ErgonomicHazardCategory
{
    /// <summary>Repetitive motion tasks.</summary>
    RepetitiveMotion = 0,

    /// <summary>Awkward posture / workstation layout.</summary>
    AwkwardPosture = 1,

    /// <summary>Heavy or manual material lifting.</summary>
    HeavyLifting = 2,
}
