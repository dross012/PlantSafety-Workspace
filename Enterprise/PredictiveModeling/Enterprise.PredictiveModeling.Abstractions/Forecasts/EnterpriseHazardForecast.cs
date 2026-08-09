// ============================================================================
//  Project:        Enterprise.PredictiveModeling
//  Module:         Abstractions.Forecasts
//  File:           EnterpriseHazardForecast.cs
//  Created:        2026-08-08
//  Description:    Phase D.8: a deterministic hazard-count forecast, built
//                  from a rolling window of historical per-period hazard
//                  counts via DeterministicSeriesMath.
// ============================================================================
namespace Enterprise.PredictiveModeling.Abstractions.Forecasts;

/// <summary>A deterministic enterprise-wide hazard-count forecast.</summary>
public sealed record EnterpriseHazardForecast(
    double MovingAverageHazardCount,
    double TrendSlope,
    double ForecastedNextPeriodHazardCount,
    bool IsAnomalousSpike,
    DateTime GeneratedAtUtc);
