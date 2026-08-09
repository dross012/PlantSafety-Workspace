// ============================================================================
//  Project:        Shared.PredictiveModels
//  Module:         Abstractions.Models
//  File:           EmployeeInjuryPredictionModel.cs
//  Created:        2026-08-08
//  Description:    Phase C.11: predicts next-period employee injury count
//                  and flags anomalous injury spikes from a rolling window
//                  of historical per-period injury counts. Deterministic:
//                  moving average + OLS trend slope + z-score anomaly
//                  detection, no ML.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

namespace Shared.PredictiveModels.Abstractions.Models;

/// <summary>The result of an <see cref="EmployeeInjuryPredictionModel"/> prediction.</summary>
public sealed record EmployeeInjuryPrediction(
    double MovingAverage,
    double TrendSlope,
    double ZScoreOfLatest,
    double PredictedNextPeriodInjuryCount,
    bool IsAnomalousSpike);

/// <summary>Predicts next-period employee injury count and flags anomalous spikes from a rolling window of historical counts.</summary>
public static class EmployeeInjuryPredictionModel
{
    /// <summary>Predicts next-period employee injury count from a rolling window of historical per-period injury counts.</summary>
    /// <param name="periodInjuryCounts">Employee injury counts per period, oldest first, including the latest.</param>
    /// <param name="windowSize">The rolling window size for the moving average.</param>
    public static EmployeeInjuryPrediction Predict(IReadOnlyList<int> periodInjuryCounts, int windowSize)
    {
        IReadOnlyList<double> series = periodInjuryCounts.Select(c => (double)c).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);
        double predicted = Math.Max(0, movingAverage + trendSlope);

        double zScore = series.Count > 1
            ? DeterministicSeriesMath.ZScore(series.Take(series.Count - 1).ToArray(), series[^1])
            : 0d;

        return new EmployeeInjuryPrediction(movingAverage, trendSlope, zScore, predicted, DeterministicSeriesMath.IsAnomalous(zScore));
    }
}
