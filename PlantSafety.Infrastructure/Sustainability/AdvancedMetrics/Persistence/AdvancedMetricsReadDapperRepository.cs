// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Infrastructure
//  File:           AdvancedMetricsReadDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the Sustainability.AdvancedMetrics
//                  Application-layer read-model. Backs the Carbon Footprint Trend, Energy/Water
//                  Intensity Board, Waste Diversion Rate, and Target Progress Tracker dashboards,
//                  plus the ESG disclosure report lookup. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Infrastructure.Sustainability.AdvancedMetrics.Persistence;

/// <summary>Dapper-backed repository for Sustainability.AdvancedMetrics dashboard and report read-model queries.</summary>
public sealed class AdvancedMetricsReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AdvancedMetricsReadDapperRepository> logger)
    : IAdvancedMetricsReadRepository
{
    /// <inheritdoc/>
    public async Task<CarbonFootprintByPeriodDto?> GetCarbonFootprintByPeriodAsync(
        Guid tenantId, Guid? siteId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("PeriodStartUtc", periodStartUtc)
            .AddDateTime("PeriodEndUtc", periodEndUtc)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CarbonFootprintByPeriodDto>(
            "[SustainabilityAdvancedMetrics].[usp_AdvancedMetrics_GetCarbonFootprintByPeriod]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnergyIntensityTrendPointDto>> GetEnergyIntensityTrendAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AdvancedMetricsLedgerId", advancedMetricsLedgerId).AddGuid("TenantId", tenantId).Build();

        return await queryExecutor.QueryAsync<EnergyIntensityTrendPointDto>(
            "[SustainabilityAdvancedMetrics].[usp_EnergyIntensityMetric_ListByLedger]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WaterIntensityTrendPointDto>> GetWaterIntensityTrendAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AdvancedMetricsLedgerId", advancedMetricsLedgerId).AddGuid("TenantId", tenantId).Build();

        return await queryExecutor.QueryAsync<WaterIntensityTrendPointDto>(
            "[SustainabilityAdvancedMetrics].[usp_WaterIntensityMetric_ListByLedger]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<WasteDiversionRateDto?> GetWasteDiversionRateAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AdvancedMetricsLedgerId", advancedMetricsLedgerId).AddGuid("TenantId", tenantId).Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<WasteDiversionRateDto>(
            "[SustainabilityAdvancedMetrics].[usp_AdvancedMetrics_GetWasteDiversionRate]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SustainabilityTargetProgressDto>> ListSustainabilityTargetsAndProgressAsync(
        Guid advancedMetricsLedgerId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AdvancedMetricsLedgerId", advancedMetricsLedgerId).AddGuid("TenantId", tenantId).Build();

        return await queryExecutor.QueryAsync<SustainabilityTargetProgressDto>(
            "[SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_ListByLedger]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EsgDisclosureReportDto?> GetEsgDisclosureReportByIdAsync(
        Guid esgDisclosureReportId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EsgDisclosureReportId", esgDisclosureReportId).AddGuid("TenantId", tenantId).Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EsgDisclosureReportDto>(
            "[SustainabilityAdvancedMetrics].[usp_EsgDisclosureReport_GetById]", parameters, cancellationToken);
    }
}
