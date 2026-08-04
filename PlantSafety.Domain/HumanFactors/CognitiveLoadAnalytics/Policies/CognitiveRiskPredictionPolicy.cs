// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy evaluating a predicted cognitive-risk level and prediction
//                  confidence score from a worker/shift's mean cognitive load and overload-event
//                  frequency. Consumed by GenerateCognitiveRiskPredictionCommandHandler via
//                  CognitiveLoadAnalyticsService. Elevated/Critical classifications are the risk
//                  hook feeding RiskManagement and AccidentManagement root-cause correlation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Policies;

/// <summary>Evaluates a predicted <see cref="CognitiveRiskLevel"/> and confidence score from workload trend inputs.</summary>
public static class CognitiveRiskPredictionPolicy
{
    /// <summary>Evaluates the predicted cognitive-risk level from a mean load score and overload frequency.</summary>
    public static CognitiveRiskLevel EvaluateRiskLevel(decimal meanCognitiveLoadScore, decimal overloadEventFrequencyPercent)
    {
        if (meanCognitiveLoadScore >= 80m || overloadEventFrequencyPercent >= 40m)
            return CognitiveRiskLevel.Critical;

        if (meanCognitiveLoadScore >= 65m || overloadEventFrequencyPercent >= 25m)
            return CognitiveRiskLevel.Elevated;

        if (meanCognitiveLoadScore >= 45m || overloadEventFrequencyPercent >= 10m)
            return CognitiveRiskLevel.Moderate;

        return CognitiveRiskLevel.Low;
    }

    /// <summary>Estimates a normalized (0-1) prediction confidence score from the same inputs used to classify risk level.</summary>
    public static decimal EstimatePredictionScore(decimal meanCognitiveLoadScore, decimal overloadEventFrequencyPercent)
        => Math.Clamp(
            Math.Round((meanCognitiveLoadScore * 0.6m + overloadEventFrequencyPercent * 0.4m) / 100m, 4, MidpointRounding.AwayFromZero),
            0m, 1m);

    /// <summary>Returns <see langword="true"/> when the given risk level warrants raising an alert to RiskManagement.</summary>
    public static bool IsAlertWorthy(CognitiveRiskLevel level) => level is CognitiveRiskLevel.Elevated or CognitiveRiskLevel.Critical;
}
