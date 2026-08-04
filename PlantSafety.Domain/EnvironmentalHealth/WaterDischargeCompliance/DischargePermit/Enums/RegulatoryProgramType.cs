// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           RegulatoryProgramType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of the regulatory program under which a DischargePermit was
//                  issued (Clean Water Act / NPDES-style permit condition mapping). Additive-only.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Enums;

/// <summary>Classification of the regulatory program under which a discharge permit was issued.</summary>
public enum RegulatoryProgramType
{
    /// <summary>Federal National Pollutant Discharge Elimination System program.</summary>
    Npdes = 0,

    /// <summary>State-delegated NPDES-equivalent program.</summary>
    StateDelegatedProgram = 1,

    /// <summary>Local municipal ordinance / pretreatment program.</summary>
    LocalOrdinance = 2,

    /// <summary>Other regulatory program not covered above.</summary>
    Other = 3,
}