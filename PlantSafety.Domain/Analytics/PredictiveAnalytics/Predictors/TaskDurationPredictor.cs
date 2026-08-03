// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           TaskDurationPredictor.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Deterministic heuristic predictor for expected task duration, used by the
//                  predictive scheduling optimization loop to size candidate assignments.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

/// <summary>Input features for <see cref="TaskDurationPredictor"/>.</summary>
public sealed record TaskDurationPredictionInput
{
    /// <summary>Mean duration observed historically for this task category.</summary>
    public required TimeSpan HistoricalAverageDuration { get; init; }

    /// <summary>Number of historical samples the average was computed from.</summary>
    public required int HistoricalSampleCount { get; init; }

    /// <summary>Relative complexity multiplier for this specific task instance (1.0 = typical).</summary>
    public double ComplexityFactor { get; init; } = 1.0;

    /// <summary>Relative crew experience multiplier; values above 1.0 indicate a more experienced crew (faster).</summary>
    public double CrewExperienceFactor { get; init; } = 1.0;
}

/// <summary>Predicted output of <see cref="TaskDurationPredictor"/>.</summary>
public sealed record TaskDurationPredictionOutput
{
    /// <summary>Predicted duration required to complete the task.</summary>
    public required TimeSpan PredictedDuration { get; init; }
}

/// <summary>
/// Deterministic heuristic predictor for expected task duration: historical average duration
/// adjusted by task complexity and crew experience. Designed to be swappable for a trained
/// regression model without changing the <see cref="IPredictionModel{TInput,TOutput}"/> contract.
/// </summary>
public sealed class TaskDurationPredictor : IPredictionModel<TaskDurationPredictionInput, TaskDurationPredictionOutput>
{
    /// <inheritdoc/>
    public string ModelName => "TaskDurationPredictor";

    /// <inheritdoc/>
    public string ModelVersion => "2026.1-heuristic";

    /// <inheritdoc/>
    public PredictionResult<TaskDurationPredictionOutput> Predict(TaskDurationPredictionInput input, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.ComplexityFactor <= 0)
            throw new ArgumentOutOfRangeException(nameof(input), "ComplexityFactor must be positive.");
        if (input.CrewExperienceFactor <= 0)
            throw new ArgumentOutOfRangeException(nameof(input), "CrewExperienceFactor must be positive.");

        var adjustedTicks = input.HistoricalAverageDuration.Ticks *
                             input.ComplexityFactor / input.CrewExperienceFactor;

        var predicted = TimeSpan.FromTicks((long)Math.Round(adjustedTicks));

        return new PredictionResult<TaskDurationPredictionOutput>
        {
            Output = new TaskDurationPredictionOutput { PredictedDuration = predicted },
            Confidence = PredictionConfidence.FromSampleCount(input.HistoricalSampleCount),
            ModelName = ModelName,
            ModelVersion = ModelVersion,
            GeneratedAtUtc = asOfUtc,
            Rationale = $"Historical average {input.HistoricalAverageDuration} scaled by complexity " +
                        $"{input.ComplexityFactor:F2} and crew experience {input.CrewExperienceFactor:F2} " +
                        $"over {input.HistoricalSampleCount} sample(s).",
        };
    }
}
