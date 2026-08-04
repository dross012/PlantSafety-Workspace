// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           IAdvancedMetricsReadRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Sustainability.AdvancedMetrics
//                  read-model dashboards (Carbon Footprint Trend, Energy/Water Intensity Board,
//                  Waste Diversion Rate, Target Progress Tracker) and the ESG disclosure report
//                  lookup. Implemented in the Infrastructure layer. Distinct from the Domain-layer
//                  write-side IAdvancedMetricsLedgerRepository / IEsgDisclosureReportRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

/// <summary>Read-only repository for Sustainability.AdvancedMetrics dashboard and report data.</summary>
public interface IAdvancedMetricsReadRepository
{
    /// <summary>Returns the carbon footprint rollup (by GHG scope) for the ledger covering the given period, or <see langword="null"/> if no ledger exists for it.</summary>
    Task<CarbonFootprintByPeriodDto?> GetCarbonFootprintByPeriodAsync(
        Guid tenantId, Guid? siteId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken);

    /// <summary>Returns the energy-intensity trend points for the given ledger, ordered by reading time.</summary>
    Task<IReadOnlyList<EnergyIntensityTrendPointDto>> GetEnergyIntensityTrendAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the water-intensity trend points for the given ledger, ordered by reading time.</summary>
    Task<IReadOnlyList<WaterIntensityTrendPointDto>> GetWaterIntensityTrendAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the waste diversion rate rollup for the given ledger.</summary>
    Task<WasteDiversionRateDto?> GetWasteDiversionRateAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns all sustainability targets and their tracked progress for the given ledger.</summary>
    Task<IReadOnlyList<SustainabilityTargetProgressDto>> ListSustainabilityTargetsAndProgressAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns a single ESG disclosure report read-model record by identity, scoped to the given tenant.</summary>
    Task<EsgDisclosureReportDto?> GetEsgDisclosureReportByIdAsync(
        Guid esgDisclosureReportId, Guid tenantId, CancellationToken cancellationToken);
}
