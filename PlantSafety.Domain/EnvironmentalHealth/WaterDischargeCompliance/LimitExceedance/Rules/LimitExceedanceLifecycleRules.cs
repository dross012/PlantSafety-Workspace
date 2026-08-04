// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the LimitExceedance aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="LimitExceedance"/> aggregate.</summary>
public static class LimitExceedanceLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a response may be initiated.</summary>
    public static bool CanInitiateResponse(LimitExceedanceStatus current) => current == LimitExceedanceStatus.Detected;

    /// <summary>Returns <see langword="true"/> when the response may be closed.</summary>
    public static bool CanCloseResponse(LimitExceedanceStatus current) => current == LimitExceedanceStatus.ResponseInitiated;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state.</summary>
    public static bool IsTerminal(LimitExceedanceStatus status) => status == LimitExceedanceStatus.Closed;

    /// <summary>Returns <see langword="true"/> when the elevated-inspection-frequency risk hook should
    /// fire because this is a repeat exceedance (data-governance / risk-hook rule: repeat exceedances
    /// trigger elevated inspection frequency).</summary>
    public static bool RequiresElevatedInspectionFrequency(int repeatExceedanceCount) => repeatExceedanceCount >= 2;
}