// ============================================================================
//  Project:        Enterprise.PredictiveModeling
//  Module:         Abstractions.Engine
//  File:           EnterprisePredictiveModelEngine.cs
//  Created:        2026-08-08
//  Description:    Phase D.8 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): builds the four enterprise-wide
//                  forecasts Path D's spec requires, entirely by composing
//                  Shared.PredictiveModels' DeterministicSeriesMath (Phase
//                  C.11) -- no new statistical logic is introduced here,
//                  only enterprise-scoped wiring around the same moving-
//                  average/trend-slope/z-score primitives already used by
//                  every per-domain predictive model.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

using Enterprise.PredictiveModeling.Abstractions.Forecasts;

namespace Enterprise.PredictiveModeling.Abstractions.Engine;

/// <summary>Builds deterministic enterprise-wide forecasts from historical time series.</summary>
public static class EnterprisePredictiveModelEngine
{
    /// <summary>Forecasts enterprise-wide risk from a rolling window of historical per-period risk weights.</summary>
    public static EnterpriseRiskForecast ForecastRisk(IReadOnlyList<double> historicalRiskWeights, int windowSize, DateTime? nowUtc = null)
    {
        double movingAverage = DeterministicSeriesMath.MovingAverage(historicalRiskWeights, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(historicalRiskWeights);
        double forecasted = Math.Max(0, movingAverage + trendSlope);
        double zScore = historicalRiskWeights.Count > 1
            ? DeterministicSeriesMath.ZScore(historicalRiskWeights.Take(historicalRiskWeights.Count - 1).ToArray(), historicalRiskWeights[^1])
            : 0d;

        return new EnterpriseRiskForecast(movingAverage, trendSlope, forecasted, DeterministicSeriesMath.IsAnomalous(zScore), nowUtc ?? DateTime.UtcNow);
    }

    /// <summary>Forecasts enterprise-wide compliance from a rolling window of historical per-period compliance percentages.</summary>
    public static EnterpriseComplianceForecast ForecastCompliance(IReadOnlyList<double> historicalCompliancePercents, int windowSize, DateTime? nowUtc = null)
    {
        double movingAverage = DeterministicSeriesMath.MovingAverage(historicalCompliancePercents, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(historicalCompliancePercents);
        double forecasted = Math.Clamp(movingAverage + trendSlope, 0, 100);

        return new EnterpriseComplianceForecast(movingAverage, trendSlope, forecasted, nowUtc ?? DateTime.UtcNow);
    }

    /// <summary>Forecasts enterprise-wide hazard occurrence from a rolling window of historical per-period hazard counts.</summary>
    public static EnterpriseHazardForecast ForecastHazard(IReadOnlyList<int> historicalHazardCounts, int windowSize, DateTime? nowUtc = null)
    {
        IReadOnlyList<double> series = historicalHazardCounts.Select(c => (double)c).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);
        double forecasted = Math.Max(0, movingAverage + trendSlope);
        double zScore = series.Count > 1
            ? DeterministicSeriesMath.ZScore(series.Take(series.Count - 1).ToArray(), series[^1])
            : 0d;

        return new EnterpriseHazardForecast(movingAverage, trendSlope, forecasted, DeterministicSeriesMath.IsAnomalous(zScore), nowUtc ?? DateTime.UtcNow);
    }

    /// <summary>Forecasts the enterprise-wide severity trend from a rolling window of historical per-event severity weights.</summary>
    public static EnterpriseSeverityForecast ForecastSeverity(IReadOnlyList<int> historicalSeverityWeights, int windowSize, DateTime? nowUtc = null)
    {
        IReadOnlyList<double> series = historicalSeverityWeights.Select(w => (double)w).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);

        return new EnterpriseSeverityForecast(movingAverage, trendSlope, IsEscalating: trendSlope > 0d, nowUtc ?? DateTime.UtcNow);
    }
}
