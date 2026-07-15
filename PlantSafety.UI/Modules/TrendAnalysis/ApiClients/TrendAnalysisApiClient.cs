// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Trend Analysis module. Wraps
//   HTTP communication for the single top-level Trend Analysis read-model
//   endpoint exposed by PlantSafety.Api.Analytics.TrendAnalysis:
//     GET api/v1/analytics/trendanalysis
//   Shares the {TenantId, SiteId?, From, To?} query contract and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;) with every other Analytics
//   section endpoint. No HttpClient calls are permitted outside this class
//   for the Trend Analysis module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.DashboardAnalytics.ApiClients;
using PlantSafety.UI.Modules.DashboardAnalytics.Models;
using PlantSafety.UI.Modules.TrendAnalysis.Models;

namespace PlantSafety.UI.Modules.TrendAnalysis.ApiClients;

/// <summary>
/// Typed API client for the Trend Analysis module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class TrendAnalysisApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="TrendAnalysisApiClient"/>.</summary>
    public TrendAnalysisApiClient(HttpClient http, ILogger<TrendAnalysisApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the trend analysis data points for the tenant and optional site.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<TrendAnalysisPointDto>>> GetTrendAnalysisAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<TrendAnalysisPointDto>>(
            $"v1/analytics/trendanalysis?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
