// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of emergency scenarios per the PlantSafety Program Structure
//                  Emergency Preparedness component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Enums;

/// <summary>Classification of emergency scenario types.</summary>
public enum EmergencyType
{
    /// <summary>Fire emergency.</summary>
    Fire = 0,

    /// <summary>Chemical spill emergency.</summary>
    ChemicalSpill = 1,

    /// <summary>Severe weather emergency.</summary>
    SevereWeather = 2,

    /// <summary>Medical emergency.</summary>
    MedicalEmergency = 3,
}
