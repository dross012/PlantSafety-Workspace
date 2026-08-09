// ============================================================================
//  Project:        AI.PredictiveOptimization
//  Module:         Abstractions.Engine
//  File:           PredictiveOptimizationEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.6 (Path E: AI-Driven Safety Automation):
//                  builds a PredictiveOptimizationModel for every
//                  OptimizationTarget in the input, using Shared.
//                  PredictiveModels' real DeterministicSeriesMath (Phase
//                  C.11, unchanged) for moving average/trend slope/z-score
//                  -- no new statistical logic. Weighted scoring and the
//                  rule-based "recommend the highest-weighted-score
//                  target" optimization are this phase's only new logic,
//                  exactly the two "deterministic methods" the spec adds
//                  beyond Phase C.11/D.8's four.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

using AI.PredictiveOptimization.Abstractions.Models;
using AI.PredictiveOptimization.Abstractions.Optimization;

namespace AI.PredictiveOptimization.Abstractions.Engine;

/// <summary>Builds deterministic, weighted optimization models across every target dimension.</summary>
public static class PredictiveOptimizationEngine
{
    /// <summary>Optimizes every target dimension present in the input and recommends where to focus.</summary>
    /// <exception cref="ArgumentException"><paramref name="input"/> has no target dimensions.</exception>
    public static PredictiveOptimizationOutput Optimize(PredictiveOptimizationInput input, DateTime? nowUtc = null)
    {
        if (input.HistoryByTarget.Count == 0)
        {
            throw new ArgumentException("At least one optimization target's history is required.", nameof(input));
        }

        var models = new List<PredictiveOptimizationModel>();
        foreach ((OptimizationTarget target, IReadOnlyList<double> history) in input.HistoryByTarget)
        {
            double weight = input.WeightByTarget.GetValueOrDefault(target, 1.0);
            double movingAverage = DeterministicSeriesMath.MovingAverage(history, input.WindowSize);
            double trendSlope = DeterministicSeriesMath.TrendSlope(history);
            double forecastedValue = Math.Max(0, movingAverage + trendSlope);
            double weightedScore = forecastedValue * weight;
            double zScore = history.Count > 1
                ? DeterministicSeriesMath.ZScore(history.Take(history.Count - 1).ToArray(), history[^1])
                : 0d;

            models.Add(new PredictiveOptimizationModel(target, movingAverage, trendSlope, forecastedValue, weight, weightedScore, DeterministicSeriesMath.IsAnomalous(zScore)));
        }

        double compositeScore = models.Sum(m => m.WeightedScore);
        OptimizationTarget recommendedFocus = models.OrderByDescending(m => m.WeightedScore).First().Target;

        return new PredictiveOptimizationOutput(models, recommendedFocus, compositeScore, nowUtc ?? DateTime.UtcNow);
    }
}
