// ============================================================================
//  Project:        Enterprise.PredictiveModeling
//  Module:         Abstractions.Forecasts
//  File:           EnterpriseSeverityForecast.cs
//  Created:        2026-08-08
//  Description:    Phase D.8: a deterministic severity-trend forecast,
//                  built from a rolling window of historical severity
//                  weights (see Shared.FacilityDomain's Phase C.3
//                  SeverityMetadata.WeightOf) via DeterministicSeriesMath.
// ============================================================================
namespace Enterprise.PredictiveModeling.Abstractions.Forecasts;

/// <summary>A deterministic enterprise-wide severity-trend forecast.</summary>
public sealed record EnterpriseSeverityForecast(
    double MovingAverageSeverityWeight,
    double TrendSlope,
    bool IsEscalating,
    DateTime GeneratedAtUtc);
