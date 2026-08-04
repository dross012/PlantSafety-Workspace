// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the EnvironmentalHealth.WaterDischargeCompliance
//   sub-module. Targets PlantSafety.Api.EnvironmentalHealth.Controllers.V1.
//   WaterDischargeComplianceV1Controller ("api/v1/environmentalhealth/water-discharge-compliance/
//   ..."). Calls gracefully fault through ApiResponseEnvelope so the UI degrades to its Error
//   state on backend failure.
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
using PlantSafety.UI.Modules.EnvironmentalHealth.WaterDischargeCompliance.Models;

namespace PlantSafety.UI.Modules.EnvironmentalHealth.WaterDischargeCompliance.ApiClients;

/// <summary>Typed API client for the EnvironmentalHealth.WaterDischargeCompliance sub-module.</summary>
public sealed class WaterDischargeComplianceApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="WaterDischargeComplianceApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public WaterDischargeComplianceApiClient(HttpClient http, ILogger<WaterDischargeComplianceApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a single discharge permit by identity, for the permit-expiration lookup panel.</summary>
    public Task<ApiResponseEnvelope<DischargePermitDto>> GetDischargePermitByIdAsync(
        Guid dischargePermitId, CancellationToken cancellationToken = default)
        => GetAsync<DischargePermitDto>($"v1/environmentalhealth/water-discharge-compliance/discharge-permits/{dischargePermitId:D}", cancellationToken);

    /// <summary>Lists discharge points for a tenant, optionally scoped to a single site.</summary>
    public Task<ApiResponseEnvelope<ListDischargePointsBySiteResponseDto>> ListDischargePointsBySiteAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<ListDischargePointsBySiteResponseDto>($"v1/environmentalhealth/water-discharge-compliance/discharge-points{query}", cancellationToken);
    }

    /// <summary>Lists active (non-Closed) discharge limit exceedances for a tenant/site.</summary>
    public Task<ApiResponseEnvelope<ListActiveExceedancesResponseDto>> ListActiveExceedancesAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<ListActiveExceedancesResponseDto>($"v1/environmentalhealth/water-discharge-compliance/limit-exceedances/active{query}", cancellationToken);
    }

    /// <summary>Retrieves the sample history for a discharge point within a date range, for the Effluent Trend Chart.</summary>
    public Task<ApiResponseEnvelope<GetSampleHistoryByPointResponseDto>> GetSampleHistoryByPointAsync(
        Guid dischargePointId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var query = $"?From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        return GetAsync<GetSampleHistoryByPointResponseDto>(
            $"v1/environmentalhealth/water-discharge-compliance/discharge-points/{dischargePointId:D}/effluent-samples{query}", cancellationToken);
    }

    /// <summary>Lists discharge limits defined for a discharge point, as reference lines for the Effluent Trend Chart.</summary>
    public Task<ApiResponseEnvelope<ListDischargeLimitsByPointResponseDto>> ListDischargeLimitsByPointAsync(
        Guid dischargePointId, CancellationToken cancellationToken = default)
        => GetAsync<ListDischargeLimitsByPointResponseDto>(
            $"v1/environmentalhealth/water-discharge-compliance/discharge-points/{dischargePointId:D}/discharge-limits", cancellationToken);

    /// <summary>Retrieves Discharge Monitoring Reports overlapping the given period, for a tenant/site.</summary>
    public Task<ApiResponseEnvelope<GetDischargeMonitoringReportsByPeriodResponseDto>> GetDischargeMonitoringReportsByPeriodAsync(
        Guid? siteId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken = default)
    {
        var query = $"?PeriodStartUtc={Uri.EscapeDataString(periodStartUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&PeriodEndUtc={Uri.EscapeDataString(periodEndUtc.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<GetDischargeMonitoringReportsByPeriodResponseDto>(
            $"v1/environmentalhealth/water-discharge-compliance/discharge-monitoring-reports{query}", cancellationToken);
    }
}
