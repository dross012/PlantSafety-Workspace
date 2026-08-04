// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CognitiveRiskPrediction aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Exceptions;

/// <summary>Thrown when a CognitiveRiskPrediction aggregate lookup finds no match.</summary>
public sealed class CognitiveRiskPredictionNotFoundException : CognitiveLoadAnalyticsDomainException
{
    /// <summary>Creates a not-found exception for the given CognitiveRiskPrediction identity.</summary>
    public CognitiveRiskPredictionNotFoundException(CognitiveRiskPredictionId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CognitiveRiskPrediction with id '{id}' was not found.",
            "CognitiveLoadAnalytics.CognitiveRiskPrediction.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
