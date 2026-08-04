// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           ExceedanceSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Severity classification for a limit exceedance. Feeds RiskManagement risk-hook
//                  scoring; repeat exceedances trigger elevated inspection frequency. Additive-only.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Enums;

/// <summary>Severity classification for a discharge limit exceedance.</summary>
public enum ExceedanceSeverity
{
    /// <summary>Minor exceedance, low risk impact.</summary>
    Minor = 0,
    /// <summary>Moderate exceedance.</summary>
    Moderate = 1,
    /// <summary>Major exceedance, elevated regulatory and risk impact.</summary>
    Major = 2,
    /// <summary>Critical exceedance requiring immediate escalation.</summary>
    Critical = 3,
}