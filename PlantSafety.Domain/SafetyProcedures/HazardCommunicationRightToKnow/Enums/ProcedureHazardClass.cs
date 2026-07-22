// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           ProcedureHazardClass.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    GHS hazard classification for a hazard communication procedure document, per the
//                  PlantSafety Program Structure Hazard Communication & Right-to-Know component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Enums;

/// <summary>GHS hazard classification for a hazard communication procedure document.</summary>
public enum ProcedureHazardClass
{
    /// <summary>Chemical hazards.</summary>
    Chemical = 0,

    /// <summary>Physical hazards.</summary>
    Physical = 1,

    /// <summary>Biological hazards.</summary>
    Biological = 2,
}
