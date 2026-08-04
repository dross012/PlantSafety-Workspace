// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPredictionFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing CognitiveRiskPrediction
//                  aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.CognitiveRiskPrediction"/> aggregates.</summary>
public sealed class CognitiveRiskPredictionFactory : ICognitiveRiskPredictionFactory
{
    /// <inheritdoc/>
    public Aggregates.CognitiveRiskPrediction Generate(
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
        string predictionBasis)
        => Aggregates.CognitiveRiskPrediction.Generate(
            generatedBy, generatedAtUtc, correlationId, causationId, actor, tenantId, siteId,
            workerUserId, sourceLedgerId, predictedRiskLevel, predictionScore, modelVersion, predictionBasis);
}
