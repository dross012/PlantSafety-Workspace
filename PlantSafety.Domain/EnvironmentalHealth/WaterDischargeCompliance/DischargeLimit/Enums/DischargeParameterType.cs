// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeParameterType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of the effluent parameter a DischargeLimit governs. Mirrors
//                  EffluentSample.Enums.SampleParameterType by design (independent, loosely
//                  coupled sub-aggregate enums per the established MachineSafety pattern).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Enums;

/// <summary>Classification of the effluent parameter a discharge limit governs.</summary>
public enum DischargeParameterType
{
    /// <summary>pH.</summary>
    PH = 0,
    /// <summary>Biochemical Oxygen Demand.</summary>
    BOD = 1,
    /// <summary>Total Suspended Solids.</summary>
    TSS = 2,
    /// <summary>Fecal Coliform.</summary>
    FecalColiform = 3,
    /// <summary>Oil and Grease.</summary>
    OilAndGrease = 4,
    /// <summary>Effluent temperature.</summary>
    Temperature = 5,
    /// <summary>Chemical Oxygen Demand.</summary>
    ChemicalOxygenDemand = 6,
    /// <summary>Other parameter not covered above.</summary>
    Other = 7,
}