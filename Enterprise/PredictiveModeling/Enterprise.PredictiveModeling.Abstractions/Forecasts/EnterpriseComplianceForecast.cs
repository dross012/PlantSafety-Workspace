// ============================================================================
//  Project:        Enterprise.PredictiveModeling
//  Module:         Abstractions.Forecasts
//  File:           EnterpriseComplianceForecast.cs
//  Created:        2026-08-08
//  Description:    Phase D.8: a deterministic compliance-percentage
//                  forecast, built from a rolling window of historical
//                  compliance percentages via DeterministicSeriesMath.
// ============================================================================
namespace Enterprise.PredictiveModeling.Abstractions.Forecasts;

/// <summary>A deterministic enterprise-wide compliance forecast.</summary>
public sealed record EnterpriseComplianceForecast(
    double MovingAverageCompliancePercent,
    double TrendSlope,
    double ForecastedNextPeriodCompliancePercent,
    DateTime GeneratedAtUtc);
