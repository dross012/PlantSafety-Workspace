// ============================================================================
//  Project:        Shared.PredictiveModels
//  Module:         Abstractions.Models
//  File:           MachineFailurePredictionModel.cs
//  Created:        2026-08-08
//  Description:    Phase C.11: predicts next-period machine failure count
//                  and flags anomalous failure spikes from a rolling
//                  window of historical per-period failure counts.
//                  Deterministic: moving average + OLS trend slope +
//                  z-score anomaly detection, no ML.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

namespace Shared.PredictiveModels.Abstractions.Models;

/// <summary>The result of a <see cref="MachineFailurePredictionModel"/> prediction.</summary>
public sealed record MachineFailurePrediction(
    double MovingAverage,
    double TrendSlope,
    double ZScoreOfLatest,
    double PredictedNextPeriodFailureCount,
    bool IsAnomalousSpike);

/// <summary>Predicts next-period machine failure count and flags anomalous spikes from a rolling window of historical counts.</summary>
public static class MachineFailurePredictionModel
{
    /// <summary>Predicts next-period machine failure count from a rolling window of historical per-period failure counts.</summary>
    /// <param name="periodFailureCounts">Machine failure counts per period, oldest first, including the latest.</param>
    /// <param name="windowSize">The rolling window size for the moving average.</param>
    public static MachineFailurePrediction Predict(IReadOnlyList<int> periodFailureCounts, int windowSize)
    {
        IReadOnlyList<double> series = periodFailureCounts.Select(c => (double)c).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);
        double predicted = Math.Max(0, movingAverage + trendSlope);

        double zScore = series.Count > 1
            ? DeterministicSeriesMath.ZScore(series.Take(series.Count - 1).ToArray(), series[^1])
            : 0d;

        return new MachineFailurePrediction(movingAverage, trendSlope, zScore, predicted, DeterministicSeriesMath.IsAnomalous(zScore));
    }
}
