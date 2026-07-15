// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Compliance dashboard module.
//   Wraps HTTP communication for the three Compliance read-model list
//   endpoints (Regulatory Requirements, Obligations, Tracking). All methods
//   return ApiResponseEnvelope<T>. No HttpClient calls are permitted outside
//   this class for the Compliance module.
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
using PlantSafety.UI.Modules.Compliance.Models;

namespace PlantSafety.UI.Modules.Compliance.ApiClients;

/// <summary>
/// Typed API client for the Compliance dashboard module. Components MUST inject
/// this class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class ComplianceApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ComplianceApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public ComplianceApiClient(HttpClient http, ILogger<ComplianceApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves applicable Regulatory Requirements for a tenant within a UTC date range. Backed by GET <c>v1/compliance/compliance/regulatoryrequirements</c>.</summary>
    public Task<ApiResponseEnvelope<RequirementsResponseDto>> GetRequirementsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RequirementsResponseDto>(
            BuildQuery("v1/compliance/compliance/regulatoryrequirements", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Retrieves Compliance Obligations for a tenant within a UTC date range. Backed by GET <c>v1/compliance/compliance/obligations</c>.</summary>
    public Task<ApiResponseEnvelope<ObligationsResponseDto>> GetObligationsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<ObligationsResponseDto>(
            BuildQuery("v1/compliance/compliance/obligations", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Retrieves Compliance Tracking items for a tenant within a UTC date range. Backed by GET <c>v1/compliance/compliance/tracking</c>.</summary>
    public Task<ApiResponseEnvelope<TrackingResponseDto>> GetTrackingAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<TrackingResponseDto>(
            BuildQuery("v1/compliance/compliance/tracking", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Builds the canonical TenantId/SiteId/From/To query string shared by every Compliance read-model endpoint.</summary>
    private static string BuildQuery(string path, Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                    $"&From={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
        {
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return $"{path}?{query}";
    }
}
