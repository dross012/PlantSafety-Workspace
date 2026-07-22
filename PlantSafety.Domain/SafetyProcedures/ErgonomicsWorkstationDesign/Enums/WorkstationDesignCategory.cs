// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           WorkstationDesignCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of ergonomic workstation-design procedure categories per the
//                  PlantSafety Program Structure Ergonomics & Workstation Design component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Enums;

/// <summary>Classification of ergonomic workstation-design procedure categories.</summary>
public enum WorkstationDesignCategory
{
    /// <summary>Workstation height adjustment.</summary>
    WorkstationHeight = 0,

    /// <summary>Reach and layout adjustment.</summary>
    ReachAndLayout = 1,

    /// <summary>Job rotation scheme.</summary>
    JobRotation = 2,
}
