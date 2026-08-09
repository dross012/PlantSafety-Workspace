// ============================================================================
//  Project:        Shared.PredictiveModels
//  Module:         Abstractions.Models
//  File:           SeverityLikelihoodModel.cs
//  Created:        2026-08-08
//  Description:    Phase C.11: predicts whether event severity is trending
//                  toward escalation from a rolling window of historical
//                  severity weights (see Shared.FacilityDomain's Phase C.3
//                  SeverityMetadata.WeightOf, 1-4). Deterministic: moving
//                  average + OLS trend slope, no ML.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

namespace Shared.PredictiveModels.Abstractions.Models;

/// <summary>The result of a <see cref="SeverityLikelihoodModel"/> prediction.</summary>
public sealed record SeverityLikelihoodPrediction(
    double MovingAverageWeight,
    double TrendSlope,
    bool IsEscalating);

/// <summary>Predicts whether event severity is trending toward escalation from a rolling window of severity weights.</summary>
public static class SeverityLikelihoodModel
{
    /// <summary>Predicts the severity trend from a rolling window of historical severity weights (1-4, see SeverityMetadata.WeightOf).</summary>
    /// <param name="severityWeights">Severity weights per event, oldest first.</param>
    /// <param name="windowSize">The rolling window size for the moving average.</param>
    public static SeverityLikelihoodPrediction Predict(IReadOnlyList<int> severityWeights, int windowSize)
    {
        IReadOnlyList<double> series = severityWeights.Select(w => (double)w).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);

        return new SeverityLikelihoodPrediction(movingAverage, trendSlope, IsEscalating: trendSlope > 0d);
    }
}
