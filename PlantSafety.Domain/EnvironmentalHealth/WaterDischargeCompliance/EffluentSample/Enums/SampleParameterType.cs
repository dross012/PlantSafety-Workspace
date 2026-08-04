// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           SampleParameterType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of the effluent parameter measured by a sample. Additive-only.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Enums;

/// <summary>Classification of the effluent parameter measured by a sample.</summary>
public enum SampleParameterType
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