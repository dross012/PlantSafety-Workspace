// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           CarbonFootprintByPeriodDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetCarbonFootprintByPeriodQuery: total and per-scope
//                  (1/2/3) carbon emissions for a tenant/site/reporting period, plus carbon
//                  intensity per unit output. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

/// <summary>Aggregated carbon footprint for a single reporting period.</summary>
public sealed record CarbonFootprintByPeriodDto
{
    /// <summary>Source ledger identifier.</summary>
    public required Guid AdvancedMetricsLedgerId { get; init; }

    /// <summary>Reporting period start.</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end.</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Total Scope 1 emissions (metric tons CO2e).</summary>
    public decimal Scope1MetricTonsCo2e { get; init; }

    /// <summary>Total Scope 2 emissions (metric tons CO2e).</summary>
    public decimal Scope2MetricTonsCo2e { get; init; }

    /// <summary>Total Scope 3 emissions (metric tons CO2e).</summary>
    public decimal Scope3MetricTonsCo2e { get; init; }

    /// <summary>Total Scope 1+2+3 emissions (metric tons CO2e).</summary>
    public decimal TotalMetricTonsCo2e { get; init; }

    /// <summary>Carbon intensity per unit of production output, if computable for this period.</summary>
    public decimal? CarbonIntensityPerUnitOutput { get; init; }

    /// <summary>Number of individual carbon emission records contributing to this rollup.</summary>
    public int RecordCount { get; init; }
}
