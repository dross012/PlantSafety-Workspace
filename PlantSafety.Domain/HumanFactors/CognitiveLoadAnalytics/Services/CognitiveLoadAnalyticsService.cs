// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadAnalyticsService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for CognitiveLoadAnalytics. Coordinates cross-record KPI
//                  computation (mean cognitive load, overload-event frequency, predicted risk
//                  level/confidence) that spans a CognitiveLoadLedger's recorded readings and does
//                  not naturally belong on a single aggregate. Domain services must not reference
//                  infrastructure (no HTTP/SQL/logging). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Services;

/// <summary>Domain service computing cross-reading cognitive-workload KPIs from a CognitiveLoadLedger aggregate.</summary>
public class CognitiveLoadAnalyticsService : IDomainService
{
    /// <summary>Computes the mean cognitive-load score across all readings on a ledger.</summary>
    public decimal? ComputeMeanCognitiveLoad(Aggregates.CognitiveLoadLedger ledger)
    {
        if (ledger.CognitiveLoadReadings.Count == 0)
            return null;

        return Math.Round(ledger.CognitiveLoadReadings.Average(r => r.CognitiveLoadScore), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Computes the overload-event frequency, as a percentage of total readings, on a ledger.</summary>
    public decimal ComputeOverloadEventFrequencyPercent(Aggregates.CognitiveLoadLedger ledger)
    {
        if (ledger.CognitiveLoadReadings.Count == 0)
            return 0m;

        var overloadCount = ledger.CognitiveLoadReadings.Count(r => r.IsOverload);
        return Math.Round(100m * overloadCount / ledger.CognitiveLoadReadings.Count, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Evaluates the predicted cognitive-risk level for a ledger's accumulated readings via <see cref="Policies.CognitiveRiskPredictionPolicy"/>.</summary>
    public CognitiveRiskLevel EvaluatePredictedRiskLevel(Aggregates.CognitiveLoadLedger ledger)
        => Policies.CognitiveRiskPredictionPolicy.EvaluateRiskLevel(
            ComputeMeanCognitiveLoad(ledger) ?? 0m, ComputeOverloadEventFrequencyPercent(ledger));

    /// <summary>Estimates the prediction confidence score for a ledger's accumulated readings via <see cref="Policies.CognitiveRiskPredictionPolicy"/>.</summary>
    public decimal EstimatePredictionScore(Aggregates.CognitiveLoadLedger ledger)
        => Policies.CognitiveRiskPredictionPolicy.EstimatePredictionScore(
            ComputeMeanCognitiveLoad(ledger) ?? 0m, ComputeOverloadEventFrequencyPercent(ledger));
}
