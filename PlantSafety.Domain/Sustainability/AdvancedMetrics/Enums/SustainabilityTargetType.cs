// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           SustainabilityTargetType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    The KPI a SustainabilityTarget is tracking.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

/// <summary>The KPI a SustainabilityTarget is tracking.</summary>
public enum SustainabilityTargetType
{
    /// <summary>Carbon intensity per unit of output.</summary>
    CarbonIntensityPerUnitOutput = 0,

    /// <summary>Energy intensity trend.</summary>
    EnergyIntensity = 1,

    /// <summary>Water intensity trend.</summary>
    WaterIntensity = 2,

    /// <summary>Waste diversion rate.</summary>
    WasteDiversionRate = 3,

    /// <summary>Absolute Scope 1+2+3 emissions reduction.</summary>
    AbsoluteEmissionsReduction = 4,
}
