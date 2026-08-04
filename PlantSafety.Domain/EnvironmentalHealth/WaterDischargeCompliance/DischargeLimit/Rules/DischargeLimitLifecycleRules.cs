// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the DischargeLimit aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="DischargeLimit"/> aggregate.</summary>
public static class DischargeLimitLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the limit value may be revised.</summary>
    public static bool CanRevise(DischargeLimitStatus current) => current == DischargeLimitStatus.Active;

    /// <summary>Returns <see langword="true"/> when the limit may be retired.</summary>
    public static bool CanRetire(DischargeLimitStatus current) => current == DischargeLimitStatus.Active;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state.</summary>
    public static bool IsTerminal(DischargeLimitStatus status) => status == DischargeLimitStatus.Retired;
}