// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the CognitiveRiskPrediction
//                  aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.CognitiveRiskPrediction"/> aggregate.</summary>
public static class CognitiveRiskPredictionLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the prediction/alert may be acknowledged from <paramref name="current"/>.</summary>
    public static bool CanAcknowledge(CognitiveRiskPredictionStatus current) => current == CognitiveRiskPredictionStatus.Generated;

    /// <summary>Returns <see langword="true"/> when the prediction may be dismissed from <paramref name="current"/>.</summary>
    public static bool CanDismiss(CognitiveRiskPredictionStatus current) => current == CognitiveRiskPredictionStatus.Generated;
}
