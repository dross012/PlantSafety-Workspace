// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictionResult.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Canonical envelope returned by every IPredictionModel implementation,
//                  carrying the predicted output alongside deterministic provenance
//                  (model name/version, generation timestamp) and a confidence score.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

/// <summary>Canonical envelope wrapping the output of a deterministic prediction model invocation.</summary>
public sealed record PredictionResult<TOutput>
{
    /// <summary>The predicted value.</summary>
    public required TOutput Output { get; init; }

    /// <summary>Confidence the model attaches to <see cref="Output"/>.</summary>
    public required PredictionConfidence Confidence { get; init; }

    /// <summary>Stable machine-readable name of the model that produced this result.</summary>
    public required string ModelName { get; init; }

    /// <summary>Semantic version of the model/heuristic that produced this result.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>UTC instant the prediction was generated.</summary>
    public required UtcTimestamp GeneratedAtUtc { get; init; }

    /// <summary>Optional short human-readable rationale for the prediction, for audit/explainability.</summary>
    public string? Rationale { get; init; }
}
