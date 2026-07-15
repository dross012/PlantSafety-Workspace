// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Engagement Analytics module.
//   Wraps HTTP communication for the single Engagement Analytics read-model
//   endpoint exposed by PlantSafety.Api.Analytics.Engagement:
//     GET api/v1/analytics/engagement
//   Shares the {TenantId, SiteId?, From, To?} query contract with every
//   other Analytics section endpoint. No HttpClient calls are permitted
//   outside this class for the Engagement Analytics module.
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
using PlantSafety.UI.Modules.EngagementAnalytics.Models;

namespace PlantSafety.UI.Modules.EngagementAnalytics.ApiClients;

/// <summary>
/// Typed API client for the Engagement Analytics module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class EngagementAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="EngagementAnalyticsApiClient"/>.</summary>
    public EngagementAnalyticsApiClient(HttpClient http, ILogger<EngagementAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the Engagement analytics data points for the tenant and optional site.</summary>
    public Task<ApiResponseEnvelope<EngagementResponseDto>> GetEngagementAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<EngagementResponseDto>(
            $"v1/analytics/engagement?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
