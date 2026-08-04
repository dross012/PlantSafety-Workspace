// ============================================================================
// File: AdvancedMetricsRequests.cs
// Project: PlantSafety.Api
// Module: Sustainability.AdvancedMetrics
// Created: 2026-08-04
// Description: Request DTOs bound from the HTTP request body for the Sustainability.
//              AdvancedMetrics V1 controller: carbon/energy/water/waste recording, sustainability
//              target setting, and ESG disclosure report generation.
// ============================================================================

namespace PlantSafety.Api.Sustainability.AdvancedMetrics.Contracts.Requests;

/// <summary>Request body for recording a single Scope 1/2/3 carbon emission activity.</summary>
public sealed record RecordCarbonEmissionRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>GHG Protocol scope: Scope1, Scope2, or Scope3.</summary>
    public required string Scope { get; init; }

    /// <summary>Emissions activity description.</summary>
    public required string ActivityDescription { get; init; }

    /// <summary>Emissions in metric tons of CO2-equivalent.</summary>
    public required decimal EmissionsMetricTonsCo2e { get; init; }

    /// <summary>Calculation methodology version (e.g. "GHG-Protocol-2026.1").</summary>
    public required string MethodologyVersion { get; init; }

    /// <summary>UTC instant the underlying emissions activity occurred.</summary>
    public required DateTime ActivityAtUtc { get; init; }
}

/// <summary>Request body for recording an energy-intensity meter read.</summary>
public sealed record RecordEnergyIntensityMetricRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Energy consumed, in kilowatt-hours, over the reading interval.</summary>
    public required decimal EnergyConsumedKwh { get; init; }

    /// <summary>Production output units the consumption is normalized against.</summary>
    public required decimal OutputUnits { get; init; }

    /// <summary>Calculation methodology version.</summary>
    public required string MethodologyVersion { get; init; }

    /// <summary>UTC instant the meter read covers.</summary>
    public required DateTime ReadingAtUtc { get; init; }
}

/// <summary>Request body for recording a water-intensity meter read.</summary>
public sealed record RecordWaterIntensityMetricRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Water consumed, in cubic meters, over the reading interval.</summary>
    public required decimal WaterConsumedCubicMeters { get; init; }

    /// <summary>Production output units the consumption is normalized against.</summary>
    public required decimal OutputUnits { get; init; }

    /// <summary>Calculation methodology version.</summary>
    public required string MethodologyVersion { get; init; }

    /// <summary>UTC instant the meter read covers.</summary>
    public required DateTime ReadingAtUtc { get; init; }
}

/// <summary>Request body for recording a waste diversion interval.</summary>
public sealed record RecordWasteDiversionRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Total waste generated, in kilograms, over the reporting interval.</summary>
    public required decimal TotalWasteKg { get; init; }

    /// <summary>Waste diverted from landfill/incineration, in kilograms.</summary>
    public required decimal DivertedWasteKg { get; init; }

    /// <summary>Calculation methodology version.</summary>
    public required string MethodologyVersion { get; init; }

    /// <summary>UTC instant the interval covers.</summary>
    public required DateTime IntervalEndAtUtc { get; init; }
}

/// <summary>Request body for setting (or re-baselining) a sustainability KPI target.</summary>
public sealed record SetSustainabilityTargetRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end (identifies/creates the target ledger).</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>The KPI this target tracks (e.g. CarbonIntensityPerUnitOutput, WasteDiversionRate).</summary>
    public required string TargetType { get; init; }

    /// <summary>Baseline value.</summary>
    public required decimal BaselineValue { get; init; }

    /// <summary>Target value.</summary>
    public required decimal TargetValue { get; init; }

    /// <summary>Date by which the target must be reached.</summary>
    public required DateTime TargetDateUtc { get; init; }
}

/// <summary>Request body for generating a new draft ESG disclosure report.</summary>
public sealed record GenerateEsgDisclosureReportRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the source AdvancedMetricsLedger to generate the report from.</summary>
    public required Guid SourceLedgerId { get; init; }

    /// <summary>Comma-separated disclosure framework(s), e.g. "GhgProtocol, Csrd, SecClimateDisclosure".</summary>
    public required string Frameworks { get; init; }

    /// <summary>Calculation methodology version in force for this report.</summary>
    public required string MethodologyVersion { get; init; }
}
