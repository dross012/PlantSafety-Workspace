// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           ProcedureEmergencyType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of emergency response procedure scenario types per the PlantSafety
//                  Program Structure Emergency Response Planning component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Enums;

/// <summary>Classification of emergency response procedure scenario types.</summary>
public enum ProcedureEmergencyType
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
