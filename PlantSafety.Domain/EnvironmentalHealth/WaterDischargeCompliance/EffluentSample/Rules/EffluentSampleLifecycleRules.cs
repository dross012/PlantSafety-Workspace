// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EffluentSample aggregate.
//                  Corrections are append-only: the original record transitions to Corrected but
//                  is never mutated or deleted, preserving the chain-of-custody trail.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EffluentSample"/> aggregate.</summary>
public static class EffluentSampleLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the sample may be marked as corrected.</summary>
    public static bool CanMarkCorrected(EffluentSampleStatus current) => current == EffluentSampleStatus.Recorded;

    /// <summary>Returns <see langword="true"/> when the sample may be voided.</summary>
    public static bool CanVoid(EffluentSampleStatus current) => current == EffluentSampleStatus.Recorded;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state.</summary>
    public static bool IsTerminal(EffluentSampleStatus status) =>
        status is EffluentSampleStatus.Corrected or EffluentSampleStatus.Voided;
}