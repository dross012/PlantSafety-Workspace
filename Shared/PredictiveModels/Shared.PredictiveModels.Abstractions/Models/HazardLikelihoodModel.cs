// ============================================================================
//  Project:        Shared.PredictiveModels
//  Module:         Abstractions.Models
//  File:           HazardLikelihoodModel.cs
//  Created:        2026-08-08
//  Description:    Phase C.11: predicts the likelihood of another hazard
//                  event in the next period from a rolling window of
//                  per-period hazard event counts. Deterministic: moving
//                  average + OLS trend slope, no ML.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

namespace Shared.PredictiveModels.Abstractions.Models;

/// <summary>The result of a <see cref="HazardLikelihoodModel"/> prediction.</summary>
public sealed record HazardLikelihoodPrediction(
    double MovingAverage,
    double TrendSlope,
    double PredictedNextPeriodCount);

/// <summary>Predicts the likelihood of another hazard event in the next period from a rolling window of counts.</summary>
public static class HazardLikelihoodModel
{
    /// <summary>Predicts next-period hazard event count from a rolling window of historical per-period counts.</summary>
    /// <param name="periodEventCounts">Hazard event counts per period, oldest first.</param>
    /// <param name="windowSize">The rolling window size for the moving average.</param>
    public static HazardLikelihoodPrediction Predict(IReadOnlyList<int> periodEventCounts, int windowSize)
    {
        IReadOnlyList<double> series = periodEventCounts.Select(c => (double)c).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);
        double predicted = Math.Max(0, movingAverage + trendSlope);

        return new HazardLikelihoodPrediction(movingAverage, trendSlope, predicted);
    }
}
