// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           OptimalSchedulingWindowPredictor.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Model Integration)
//  SchemaVersion:  2026.1
//  Description:    Deterministic heuristic predictor that scores a set of candidate scheduling
//                  windows and recommends the most favorable one, blending resource load,
//                  historical failure rate, and shift fatigue risk. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

/// <summary>A single candidate window scored by <see cref="OptimalSchedulingWindowPredictor"/>.</summary>
public sealed record SchedulingWindowCandidate
{
    /// <summary>UTC start of the candidate window.</summary>
    public required UtcTimestamp StartUtc { get; init; }

    /// <summary>Projected resource utilization (0.0-1.0) if this window is chosen.</summary>
    public required double ProjectedUtilization { get; init; }

    /// <summary>Historical failure/incident rate (0.0-1.0) observed for work performed in this window.</summary>
    public required double HistoricalFailureRate { get; init; }

    /// <summary>Shift fatigue risk index (0.0-1.0) for the crew active during this window.</summary>
    public required double ShiftFatigueIndex { get; init; }
}

/// <summary>Input to <see cref="OptimalSchedulingWindowPredictor"/>.</summary>
public sealed record OptimalSchedulingWindowPredictionInput
{
    /// <summary>Candidate windows to evaluate; must contain at least one entry.</summary>
    public required IReadOnlyList<SchedulingWindowCandidate> Candidates { get; init; }
}

/// <summary>Predicted output of <see cref="OptimalSchedulingWindowPredictor"/>.</summary>
public sealed record OptimalSchedulingWindowPredictionOutput
{
    /// <summary>UTC start of the recommended window.</summary>
    public required UtcTimestamp RecommendedStartUtc { get; init; }

    /// <summary>Composite desirability score (higher is better) for the recommended window.</summary>
    public required double Score { get; init; }
}

/// <summary>
/// Deterministic heuristic predictor that ranks candidate scheduling windows by a weighted
/// composite of projected utilization, historical failure rate, and shift fatigue risk
/// (lower values on each dimension are more desirable).
/// </summary>
public sealed class OptimalSchedulingWindowPredictor
    : IPredictionModel<OptimalSchedulingWindowPredictionInput, OptimalSchedulingWindowPredictionOutput>
{
    private const double UtilizationWeight = 0.4;
    private const double FailureRateWeight = 0.4;
    private const double FatigueWeight = 0.2;

    /// <inheritdoc/>
    public string ModelName => "OptimalSchedulingWindowPredictor";

    /// <inheritdoc/>
    public string ModelVersion => "2026.1-heuristic";

    /// <inheritdoc/>
    public PredictionResult<OptimalSchedulingWindowPredictionOutput> Predict(
        OptimalSchedulingWindowPredictionInput input, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Candidates is null || input.Candidates.Count == 0)
            throw new ArgumentException("At least one candidate window is required.", nameof(input));

        var scored = input.Candidates
            .Select(c => (Candidate: c, Score: ComputeDesirability(c)))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Candidate.StartUtc.Value)
            .First();

        return new PredictionResult<OptimalSchedulingWindowPredictionOutput>
        {
            Output = new OptimalSchedulingWindowPredictionOutput
            {
                RecommendedStartUtc = scored.Candidate.StartUtc,
                Score = Math.Round(scored.Score, 4),
            },
            Confidence = PredictionConfidence.FromSampleCount(input.Candidates.Count, samplesForFullConfidence: 10),
            ModelName = ModelName,
            ModelVersion = ModelVersion,
            GeneratedAtUtc = asOfUtc,
            Rationale = $"Selected best of {input.Candidates.Count} candidate window(s) by weighted " +
                        "utilization/failure-rate/fatigue composite.",
        };
    }

    /// <summary>Computes the desirability score (higher is better) for a single candidate window.</summary>
    private static double ComputeDesirability(SchedulingWindowCandidate candidate)
    {
        var penalty =
            (Math.Clamp(candidate.ProjectedUtilization, 0.0, 1.0) * UtilizationWeight) +
            (Math.Clamp(candidate.HistoricalFailureRate, 0.0, 1.0) * FailureRateWeight) +
            (Math.Clamp(candidate.ShiftFatigueIndex, 0.0, 1.0) * FatigueWeight);

        return 1.0 - penalty;
    }
}
