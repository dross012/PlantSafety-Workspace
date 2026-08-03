// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           ResourceConflictPredictor.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Deterministic heuristic predictor estimating the likelihood a proposed
//                  resource assignment will collide with other demand for the same resource,
//                  used by the optimization loop to steer candidate windows away from hot spots.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

/// <summary>Input features for <see cref="ResourceConflictPredictor"/>.</summary>
public sealed record ResourceConflictPredictionInput
{
    /// <summary>Number of other commitments already competing for the same window.</summary>
    public required int CompetingCommitmentsInWindow { get; init; }

    /// <summary>Historical utilization ratio (0.0-1.0) of the resource around this time of day/week.</summary>
    public required double HistoricalUtilizationRatio { get; init; }

    /// <summary>Number of historical samples backing <see cref="HistoricalUtilizationRatio"/>.</summary>
    public required int HistoricalSampleCount { get; init; }
}

/// <summary>Predicted output of <see cref="ResourceConflictPredictor"/>.</summary>
public sealed record ResourceConflictPredictionOutput
{
    /// <summary>Estimated likelihood (0.0-1.0) that this window will produce a resource conflict.</summary>
    public required double ConflictLikelihood { get; init; }
}

/// <summary>
/// Deterministic heuristic predictor for resource conflict likelihood: blends the count of
/// directly competing commitments with the resource's historical utilization at that window.
/// </summary>
public sealed class ResourceConflictPredictor : IPredictionModel<ResourceConflictPredictionInput, ResourceConflictPredictionOutput>
{
    private const double CommitmentWeight = 0.6;
    private const double UtilizationWeight = 0.4;

    /// <inheritdoc/>
    public string ModelName => "ResourceConflictPredictor";

    /// <inheritdoc/>
    public string ModelVersion => "2026.1-heuristic";

    /// <inheritdoc/>
    public PredictionResult<ResourceConflictPredictionOutput> Predict(ResourceConflictPredictionInput input, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.CompetingCommitmentsInWindow < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "CompetingCommitmentsInWindow must not be negative.");

        var commitmentScore = 1.0 - Math.Pow(0.5, input.CompetingCommitmentsInWindow);
        var utilizationScore = Math.Clamp(input.HistoricalUtilizationRatio, 0.0, 1.0);

        var likelihood = Math.Clamp(
            (commitmentScore * CommitmentWeight) + (utilizationScore * UtilizationWeight), 0.0, 1.0);

        return new PredictionResult<ResourceConflictPredictionOutput>
        {
            Output = new ResourceConflictPredictionOutput { ConflictLikelihood = Math.Round(likelihood, 4) },
            Confidence = PredictionConfidence.FromSampleCount(input.HistoricalSampleCount),
            ModelName = ModelName,
            ModelVersion = ModelVersion,
            GeneratedAtUtc = asOfUtc,
            Rationale = $"{input.CompetingCommitmentsInWindow} competing commitment(s), " +
                        $"{input.HistoricalUtilizationRatio:P0} historical utilization.",
        };
    }
}
