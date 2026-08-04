// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CognitiveRiskPrediction lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Exceptions;

/// <summary>Thrown when a CognitiveRiskPrediction lifecycle transition is not permitted.</summary>
public sealed class CognitiveRiskPredictionInvalidStatusTransitionException : CognitiveLoadAnalyticsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CognitiveRiskPredictionInvalidStatusTransitionException(
        CognitiveRiskPredictionId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'CognitiveRiskPrediction' {id} from '{from}' to '{to}'.",
            "CognitiveLoadAnalytics.CognitiveRiskPrediction.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
