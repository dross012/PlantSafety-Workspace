// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           GhsHazardClass.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    GHS hazard classification for HazCom training topics, per the PlantSafety Program
//                  Structure Hazard Communication & Training component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Enums;

/// <summary>GHS hazard classification.</summary>
public enum GhsHazardClass
{
    /// <summary>Chemical hazards (flammable, corrosive, toxic, reactive).</summary>
    Chemical = 0,

    /// <summary>Physical hazards (noise, heat, mechanical, electrical).</summary>
    Physical = 1,

    /// <summary>Biological hazards (bloodborne pathogens, mold, bacteria).</summary>
    Biological = 2,
}
