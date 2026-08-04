// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the Sustainability.AdvancedMetrics sub-module. Targets
//   PlantSafety.Api.Sustainability.AdvancedMetrics.Controllers.V1.AdvancedMetricsV1Controller
//   ("api/v1/sustainability/advanced-metrics/..."). Calls gracefully fault through
//   ApiResponseEnvelope so the UI degrades to its Error state on backend failure.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.UI.Modules.Sustainability.AdvancedMetrics.ApiClients;

/// <summary>Typed API client for the Sustainability.AdvancedMetrics sub-module.</summary>
public sealed class AdvancedMetricsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="AdvancedMetricsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AdvancedMetricsApiClient(HttpClient http, ILogger<AdvancedMetricsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the Scope 1/2/3 carbon footprint rollup for a reporting period.</summary>
    public Task<ApiResponseEnvelope<CarbonFootprintByPeriodDto>> GetCarbonFootprintByPeriodAsync(
        Guid? siteId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken = default)
    {
        var query = $"periodStartUtc={Uri.EscapeDataString(periodStartUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&periodEndUtc={Uri.EscapeDataString(periodEndUtc.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<CarbonFootprintByPeriodDto>($"v1/sustainability/advanced-metrics/carbon-footprint?{query}", cancellationToken);
    }

    /// <summary>Retrieves the energy-intensity trend points for a ledger.</summary>
    public Task<ApiResponseEnvelope<System.Collections.Generic.IReadOnlyList<EnergyIntensityTrendPointDto>>> GetEnergyIntensityTrendAsync(
        Guid advancedMetricsLedgerId, CancellationToken cancellationToken = default)
        => GetAsync<System.Collections.Generic.IReadOnlyList<EnergyIntensityTrendPointDto>>(
            $"v1/sustainability/advanced-metrics/{advancedMetricsLedgerId:D}/energy-intensity-trend", cancellationToken);

    /// <summary>Retrieves the water-intensity trend points for a ledger.</summary>
    public Task<ApiResponseEnvelope<System.Collections.Generic.IReadOnlyList<WaterIntensityTrendPointDto>>> GetWaterIntensityTrendAsync(
        Guid advancedMetricsLedgerId, CancellationToken cancellationToken = default)
        => GetAsync<System.Collections.Generic.IReadOnlyList<WaterIntensityTrendPointDto>>(
            $"v1/sustainability/advanced-metrics/{advancedMetricsLedgerId:D}/water-intensity-trend", cancellationToken);

    /// <summary>Retrieves the waste diversion rate rollup for a ledger.</summary>
    public Task<ApiResponseEnvelope<WasteDiversionRateDto>> GetWasteDiversionRateAsync(
        Guid advancedMetricsLedgerId, CancellationToken cancellationToken = default)
        => GetAsync<WasteDiversionRateDto>($"v1/sustainability/advanced-metrics/{advancedMetricsLedgerId:D}/waste-diversion-rate", cancellationToken);

    /// <summary>Retrieves all sustainability targets and their tracked progress for a ledger.</summary>
    public Task<ApiResponseEnvelope<System.Collections.Generic.IReadOnlyList<SustainabilityTargetProgressDto>>> ListSustainabilityTargetsAndProgressAsync(
        Guid advancedMetricsLedgerId, CancellationToken cancellationToken = default)
        => GetAsync<System.Collections.Generic.IReadOnlyList<SustainabilityTargetProgressDto>>(
            $"v1/sustainability/advanced-metrics/{advancedMetricsLedgerId:D}/targets", cancellationToken);

    /// <summary>Retrieves a single ESG disclosure report by identity.</summary>
    public Task<ApiResponseEnvelope<EsgDisclosureReportDto>> GetEsgDisclosureReportByIdAsync(
        Guid esgDisclosureReportId, CancellationToken cancellationToken = default)
        => GetAsync<EsgDisclosureReportDto>($"v1/sustainability/advanced-metrics/disclosure-reports/{esgDisclosureReportId:D}", cancellationToken);
}
