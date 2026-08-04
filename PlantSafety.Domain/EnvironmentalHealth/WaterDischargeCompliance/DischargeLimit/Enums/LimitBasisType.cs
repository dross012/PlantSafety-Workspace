// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           LimitBasisType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Statistical basis over which a DischargeLimit is evaluated. Additive-only.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Enums;

/// <summary>Statistical basis over which a discharge limit is evaluated.</summary>
public enum LimitBasisType
{
    /// <summary>Daily maximum.</summary>
    DailyMaximum = 0,
    /// <summary>Monthly average.</summary>
    MonthlyAverage = 1,
    /// <summary>Weekly average.</summary>
    WeeklyAverage = 2,
    /// <summary>Instantaneous maximum.</summary>
    InstantaneousMaximum = 3,
}