// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the Sustainability.AdvancedMetrics sub-module. Mirrors
//   the wire shape of PlantSafety.Application.Sustainability.AdvancedMetrics.Models.*, returned by
//   PlantSafety.Api.Sustainability.AdvancedMetrics.Controllers.V1.AdvancedMetricsV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Sustainability.AdvancedMetrics.Models;

/// <summary>Aggregated carbon footprint for a single reporting period (Carbon Footprint Trend dashboard).</summary>
public sealed class CarbonFootprintByPeriodDto
{
    /// <summary>Gets the source ledger identifier.</summary>
    public Guid AdvancedMetricsLedgerId { get; init; }

    /// <summary>Gets the reporting period start.</summary>
    public DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Gets the reporting period end.</summary>
    public DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Gets total Scope 1 emissions (metric tons CO2e).</summary>
    public decimal Scope1MetricTonsCo2e { get; init; }

    /// <summary>Gets total Scope 2 emissions (metric tons CO2e).</summary>
    public decimal Scope2MetricTonsCo2e { get; init; }

    /// <summary>Gets total Scope 3 emissions (metric tons CO2e).</summary>
    public decimal Scope3MetricTonsCo2e { get; init; }

    /// <summary>Gets total Scope 1+2+3 emissions (metric tons CO2e).</summary>
    public decimal TotalMetricTonsCo2e { get; init; }

    /// <summary>Gets carbon intensity per unit of production output, if computable.</summary>
    public decimal? CarbonIntensityPerUnitOutput { get; init; }

    /// <summary>Gets the number of individual carbon emission records contributing to this rollup.</summary>
    public int RecordCount { get; init; }
}

/// <summary>A single energy-intensity trend data point (Energy/Water Intensity Board dashboard).</summary>
public sealed class EnergyIntensityTrendPointDto
{
    /// <summary>Gets the record identifier.</summary>
    public Guid EnergyIntensityMetricId { get; init; }

    /// <summary>Gets the UTC instant of the underlying meter read.</summary>
    public DateTime ReadingAtUtc { get; init; }

    /// <summary>Gets energy consumed, in kilowatt-hours.</summary>
    public decimal EnergyConsumedKwh { get; init; }

    /// <summary>Gets the production output units the consumption is normalized against.</summary>
    public decimal OutputUnits { get; init; }

    /// <summary>Gets the computed intensity value.</summary>
    public decimal IntensityValue { get; init; }

    /// <summary>Gets the calculation methodology version in force when this point was recorded.</summary>
    public string MethodologyVersion { get; init; } = string.Empty;
}

/// <summary>A single water-intensity trend data point (Energy/Water Intensity Board dashboard).</summary>
public sealed class WaterIntensityTrendPointDto
{
    /// <summary>Gets the record identifier.</summary>
    public Guid WaterIntensityMetricId { get; init; }

    /// <summary>Gets the UTC instant of the underlying meter read.</summary>
    public DateTime ReadingAtUtc { get; init; }

    /// <summary>Gets water consumed, in cubic meters.</summary>
    public decimal WaterConsumedCubicMeters { get; init; }

    /// <summary>Gets the production output units the consumption is normalized against.</summary>
    public decimal OutputUnits { get; init; }

    /// <summary>Gets the computed intensity value.</summary>
    public decimal IntensityValue { get; init; }

    /// <summary>Gets the calculation methodology version in force when this point was recorded.</summary>
    public string MethodologyVersion { get; init; } = string.Empty;
}

/// <summary>Aggregated waste diversion rate for a single reporting period (Waste Diversion Rate dashboard).</summary>
public sealed class WasteDiversionRateDto
{
    /// <summary>Gets the source ledger identifier.</summary>
    public Guid AdvancedMetricsLedgerId { get; init; }

    /// <summary>Gets total waste generated, in kilograms.</summary>
    public decimal TotalWasteKg { get; init; }

    /// <summary>Gets total waste diverted from landfill/incineration, in kilograms.</summary>
    public decimal DivertedWasteKg { get; init; }

    /// <summary>Gets the overall diversion rate as a percentage (0-100).</summary>
    public decimal DiversionRatePercent { get; init; }

    /// <summary>Gets the number of individual waste diversion records contributing to this rollup.</summary>
    public int RecordCount { get; init; }
}

/// <summary>A single sustainability target and its tracked progress (Target Progress Tracker dashboard).</summary>
public sealed class SustainabilityTargetProgressDto
{
    /// <summary>Gets the target identifier.</summary>
    public Guid SustainabilityTargetId { get; init; }

    /// <summary>Gets the owning ledger identifier.</summary>
    public Guid AdvancedMetricsLedgerId { get; init; }

    /// <summary>Gets the KPI this target tracks.</summary>
    public string TargetType { get; init; } = string.Empty;

    /// <summary>Gets the baseline value.</summary>
    public decimal BaselineValue { get; init; }

    /// <summary>Gets the target value.</summary>
    public decimal TargetValue { get; init; }

    /// <summary>Gets the most recently observed value.</summary>
    public decimal CurrentValue { get; init; }

    /// <summary>Gets the date by which the target must be reached.</summary>
    public DateTime TargetDateUtc { get; init; }

    /// <summary>Gets the current progress status (OnTrack/AtRisk/Achieved/Missed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant of the most recent progress update, if any.</summary>
    public DateTime? LastProgressUpdateAtUtc { get; init; }
}

/// <summary>A single ESG disclosure report read-model record.</summary>
public sealed class EsgDisclosureReportDto
{
    /// <summary>Gets the report identifier.</summary>
    public Guid EsgDisclosureReportId { get; init; }

    /// <summary>Gets the source ledger identifier this report was generated from.</summary>
    public Guid SourceLedgerId { get; init; }

    /// <summary>Gets the disclosure framework(s) this report supports.</summary>
    public string Frameworks { get; init; } = string.Empty;

    /// <summary>Gets the report version number.</summary>
    public int Version { get; init; }

    /// <summary>Gets the identifier of the report this version supersedes, if any.</summary>
    public Guid? SupersedesReportId { get; init; }

    /// <summary>Gets the current lifecycle status (Draft/Published/Superseded).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the report was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the report was published, if it has been.</summary>
    public DateTime? PublishedAtUtc { get; init; }
}
