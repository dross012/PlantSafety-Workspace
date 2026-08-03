// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           FailureLikelihoodPredictor.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Deterministic heuristic predictor estimating equipment failure likelihood,
//                  feeding both PredictiveSafetyAlert creation and the predictive scheduling
//                  reprioritization pipeline. Implements the shared IPredictionModel contract
//                  defined in Analytics.PredictiveAnalytics.Predictors. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

namespace PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors;

/// <summary>Input features for <see cref="FailureLikelihoodPredictor"/>.</summary>
public sealed record FailureLikelihoodPredictionInput
{
    /// <summary>Equipment age in months since commissioning.</summary>
    public required int EquipmentAgeMonths { get; init; }

    /// <summary>Run hours accumulated since the last completed preventive maintenance.</summary>
    public required double RunHoursSinceLastPm { get; init; }

    /// <summary>Expected safe run-hour interval between PM cycles for this equipment class.</summary>
    public required double ExpectedPmIntervalHours { get; init; }

    /// <summary>Count of failure/breakdown events recorded in the trailing 90 days.</summary>
    public required int HistoricalFailureCount90Days { get; init; }

    /// <summary>Normalized sensor/vibration anomaly score (0.0-1.0; 0 = nominal, 1 = severe anomaly).</summary>
    public double AnomalyScore { get; init; }
}

/// <summary>Predicted output of <see cref="FailureLikelihoodPredictor"/>.</summary>
public sealed record FailureLikelihoodPredictionOutput
{
    /// <summary>Estimated likelihood (0.0-1.0) of failure before the next scheduled PM window.</summary>
    public required double FailureLikelihood { get; init; }

    /// <summary>True when the likelihood crosses the platform alerting threshold.</summary>
    public required bool ShouldRaiseAlert { get; init; }
}

/// <summary>
/// Deterministic heuristic predictor for equipment failure likelihood: a logistic-shaped blend
/// of PM interval overrun, equipment age, recent failure history, and sensor anomaly score.
/// Designed to be swapped for a trained survival/classification model without changing callers.
/// </summary>
public sealed class FailureLikelihoodPredictor : IPredictionModel<FailureLikelihoodPredictionInput, FailureLikelihoodPredictionOutput>
{
    /// <summary>Failure-likelihood threshold above which a PredictiveSafetyAlert should be raised.</summary>
    public const double AlertThreshold = 0.65;

    /// <summary>
    /// Relative weight of PM-interval overrun in the blended failure-likelihood score. Exposed
    /// publicly (Phase 5 UI Gaps pass, 2026-08-03) so Application-layer consumers can surface the
    /// real model weights as explainability/feature-importance data (e.g. Operational
    /// .PredictiveMaintenance's model-explanation query) instead of fabricating separate figures.
    /// </summary>
    public const double PmOverrunWeight = 0.35;

    /// <summary>Relative weight of equipment age in the blended failure-likelihood score. See <see cref="PmOverrunWeight"/> remarks.</summary>
    public const double AgeWeight = 0.15;

    /// <summary>Relative weight of trailing-90-day failure history in the blended failure-likelihood score. See <see cref="PmOverrunWeight"/> remarks.</summary>
    public const double HistoricalFailureWeight = 0.25;

    /// <summary>Relative weight of the sensor/vibration anomaly score in the blended failure-likelihood score. See <see cref="PmOverrunWeight"/> remarks.</summary>
    public const double AnomalyWeight = 0.25;

    /// <inheritdoc/>
    public string ModelName => "FailureLikelihoodPredictor";

    /// <inheritdoc/>
    public string ModelVersion => "2026.1-heuristic";

    /// <inheritdoc/>
    public PredictionResult<FailureLikelihoodPredictionOutput> Predict(FailureLikelihoodPredictionInput input, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.ExpectedPmIntervalHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(input), "ExpectedPmIntervalHours must be positive.");
        if (input.EquipmentAgeMonths < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "EquipmentAgeMonths must not be negative.");

        var pmOverrunRatio = Math.Clamp(input.RunHoursSinceLastPm / input.ExpectedPmIntervalHours, 0.0, 2.0) / 2.0;
        var ageScore = Math.Clamp(input.EquipmentAgeMonths / 240.0, 0.0, 1.0); // saturates at 20 years
        var historicalScore = 1.0 - Math.Pow(0.6, input.HistoricalFailureCount90Days);
        var anomalyScore = Math.Clamp(input.AnomalyScore, 0.0, 1.0);

        var likelihood = Math.Clamp(
            (pmOverrunRatio * PmOverrunWeight) +
            (ageScore * AgeWeight) +
            (historicalScore * HistoricalFailureWeight) +
            (anomalyScore * AnomalyWeight), 0.0, 1.0);

        likelihood = Math.Round(likelihood, 4);

        return new PredictionResult<FailureLikelihoodPredictionOutput>
        {
            Output = new FailureLikelihoodPredictionOutput
            {
                FailureLikelihood = likelihood,
                ShouldRaiseAlert = likelihood >= AlertThreshold,
            },
            Confidence = PredictionConfidence.FromSampleCount(
                input.HistoricalFailureCount90Days > 0 ? 20 : 8, samplesForFullConfidence: 20),
            ModelName = ModelName,
            ModelVersion = ModelVersion,
            GeneratedAtUtc = asOfUtc,
            Rationale = $"PM overrun ratio {pmOverrunRatio:P0}, age score {ageScore:P0}, " +
                        $"{input.HistoricalFailureCount90Days} recent failure(s), anomaly score {anomalyScore:P0}.",
        };
    }
}
