// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           ICognitiveRiskPredictionFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing CognitiveRiskPrediction aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.CognitiveRiskPrediction"/> aggregates.</summary>
public interface ICognitiveRiskPredictionFactory
{
    /// <summary>Generates a new CognitiveRiskPrediction with the supplied parameters.</summary>
    Aggregates.CognitiveRiskPrediction Generate(
        UserId generatedBy,
        UtcTimestamp generatedAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        UserId workerUserId,
        CognitiveLoadLedgerId sourceLedgerId,
        CognitiveRiskLevel predictedRiskLevel,
        decimal predictionScore,
        string modelVersion,
        string predictionBasis);
}
