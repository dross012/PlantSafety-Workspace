// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for AdvancedMetrics. Coordinates cross-aggregate KPI computation
//                  (carbon intensity per unit output, energy/water intensity trend, waste diversion
//                  rate, percent of targets on track) that spans multiple AdvancedMetricsLedger
//                  records and does not naturally belong on a single aggregate. Domain services must
//                  not reference infrastructure (no HTTP/SQL/logging).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Services;

/// <summary>Domain service computing cross-record ESG KPIs from one or more AdvancedMetricsLedger aggregates.</summary>
public class AdvancedMetricsService : IDomainService
{
    /// <summary>Computes total Scope 1+2+3 carbon emissions (metric tons CO2e) recorded on a ledger.</summary>
    public decimal ComputeTotalCarbonEmissions(Aggregates.AdvancedMetricsLedger ledger)
        => ledger.CarbonEmissionRecords.Sum(r => r.EmissionsMetricTonsCo2e);

    /// <summary>Computes carbon intensity per unit output: total Scope 1+2+3 emissions divided by total energy-metric output units.</summary>
    public decimal? ComputeCarbonIntensityPerUnitOutput(Aggregates.AdvancedMetricsLedger ledger)
    {
        var totalOutputUnits = ledger.EnergyIntensityMetrics.Sum(m => m.OutputUnits);
        if (totalOutputUnits <= 0)
            return null;

        return Math.Round(ComputeTotalCarbonEmissions(ledger) / totalOutputUnits, 6, MidpointRounding.AwayFromZero);
    }

    /// <summary>Computes the average waste diversion rate (%) across all waste diversion records on a ledger.</summary>
    public decimal? ComputeAverageWasteDiversionRate(Aggregates.AdvancedMetricsLedger ledger)
    {
        if (ledger.WasteDiversionRecords.Count == 0)
            return null;

        return Math.Round(ledger.WasteDiversionRecords.Average(r => r.DiversionRatePercent), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Computes the percentage of sustainability targets currently classified as OnTrack or Achieved.</summary>
    public decimal? ComputePercentTargetsOnTrack(Aggregates.AdvancedMetricsLedger ledger)
    {
        if (ledger.SustainabilityTargets.Count == 0)
            return null;

        var onTrackCount = ledger.SustainabilityTargets.Count(t =>
            t.Status is SustainabilityTargetStatus.OnTrack or SustainabilityTargetStatus.Achieved);

        return Math.Round(100m * onTrackCount / ledger.SustainabilityTargets.Count, 2, MidpointRounding.AwayFromZero);
    }
}
