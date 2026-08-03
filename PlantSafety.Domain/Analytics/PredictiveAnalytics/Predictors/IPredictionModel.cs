// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           IPredictionModel.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Canonical contract for a deterministic, testable prediction model.
//                  Implementations MUST be pure functions of their input (no live ML
//                  runtime calls, no infrastructure I/O) so they remain unit-testable and
//                  swappable for a trained model without touching consuming code.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

/// <summary>Canonical contract for a deterministic prediction model behind a clean, swappable abstraction.</summary>
public interface IPredictionModel<in TInput, TOutput>
{
    /// <summary>Stable machine-readable model name (e.g. "TaskDurationPredictor").</summary>
    string ModelName { get; }

    /// <summary>Semantic version of the current heuristic/model implementation.</summary>
    string ModelVersion { get; }

    /// <summary>Produces a deterministic prediction for the given <paramref name="input"/> as of <paramref name="asOfUtc"/>.</summary>
    PredictionResult<TOutput> Predict(TInput input, UtcTimestamp asOfUtc);
}
