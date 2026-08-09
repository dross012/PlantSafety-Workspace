// ============================================================================
//  Project:        Shared.PredictiveModels
//  Module:         Abstractions.Models
//  File:           ExposureLikelihoodModel.cs
//  Created:        2026-08-08
//  Description:    Phase C.11: predicts whether the latest period's
//                  employee hazard-exposure count is anomalous against its
//                  rolling history. Deterministic: moving average +
//                  z-score anomaly detection, no ML.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

namespace Shared.PredictiveModels.Abstractions.Models;

/// <summary>The result of an <see cref="ExposureLikelihoodModel"/> prediction.</summary>
public sealed record ExposureLikelihoodPrediction(
    double MovingAverage,
    double ZScoreOfLatest,
    bool IsAnomalous);

/// <summary>Predicts whether the latest employee exposure count is anomalous against its rolling history.</summary>
public static class ExposureLikelihoodModel
{
    /// <summary>Predicts the exposure anomaly status of the latest period against a rolling window of prior periods.</summary>
    /// <param name="priorPeriodExposureCounts">Employee exposure counts for prior periods, oldest first, excluding the latest.</param>
    /// <param name="latestPeriodExposureCount">The exposure count for the period being evaluated.</param>
    /// <param name="windowSize">The rolling window size for the moving average.</param>
    public static ExposureLikelihoodPrediction Predict(IReadOnlyList<int> priorPeriodExposureCounts, int latestPeriodExposureCount, int windowSize)
    {
        IReadOnlyList<double> series = priorPeriodExposureCounts.Select(c => (double)c).ToArray();
        double movingAverage = series.Count == 0 ? 0d : DeterministicSeriesMath.MovingAverage(series, windowSize);
        double zScore = DeterministicSeriesMath.ZScore(series, latestPeriodExposureCount);

        return new ExposureLikelihoodPrediction(movingAverage, zScore, DeterministicSeriesMath.IsAnomalous(zScore));
    }
}
