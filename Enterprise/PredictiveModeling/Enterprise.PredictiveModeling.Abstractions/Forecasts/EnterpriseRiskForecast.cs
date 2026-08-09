// ============================================================================
//  Project:        Enterprise.PredictiveModeling
//  Module:         Abstractions.Forecasts
//  File:           EnterpriseRiskForecast.cs
//  Created:        2026-08-08
//  Description:    Phase D.8 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): a deterministic risk forecast, built
//                  from a rolling window of historical risk weights via
//                  Shared.PredictiveModels' DeterministicSeriesMath
//                  (Phase C.11) -- moving average, OLS trend slope, and
//                  z-score anomaly detection. No ML, no randomness.
// ============================================================================
namespace Enterprise.PredictiveModeling.Abstractions.Forecasts;

/// <summary>A deterministic enterprise-wide risk forecast.</summary>
public sealed record EnterpriseRiskForecast(
    double MovingAverageRiskWeight,
    double TrendSlope,
    double ForecastedNextPeriodRiskWeight,
    bool IsAnomalousSpike,
    DateTime GeneratedAtUtc);
