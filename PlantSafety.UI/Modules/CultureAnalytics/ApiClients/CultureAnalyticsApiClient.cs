// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Culture Analytics module.
//   Wraps HTTP communication for the four Culture Analytics read-model
//   sections exposed by PlantSafety.Api.Analytics.Culture.*:
//     GET api/v1/analytics/culture/behavior
//     GET api/v1/analytics/culture/cultureindex
//     GET api/v1/analytics/culture/engagementscoring
//     GET api/v1/analytics/culture/trendmodeling
//   Shares the {TenantId, SiteId?, From, To?} query contract and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;) with every other Analytics
//   section endpoint. No HttpClient calls are permitted outside this class
//   for the Culture Analytics module.
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
using PlantSafety.UI.Modules.CultureAnalytics.Models;
using PlantSafety.UI.Modules.DashboardAnalytics.ApiClients;
using PlantSafety.UI.Modules.DashboardAnalytics.Models;

namespace PlantSafety.UI.Modules.CultureAnalytics.ApiClients;

/// <summary>
/// Typed API client for the Culture Analytics module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class CultureAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="CultureAnalyticsApiClient"/>.</summary>
    public CultureAnalyticsApiClient(HttpClient http, ILogger<CultureAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the Behavior observation section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<BehaviorObservationDto>>> GetBehaviorAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<BehaviorObservationDto>>(
            $"v1/analytics/culture/behavior?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Culture Index section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<CultureIndexSnapshotDto>>> GetCultureIndexAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<CultureIndexSnapshotDto>>(
            $"v1/analytics/culture/cultureindex?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Engagement Scoring section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<EngagementScoreDto>>> GetEngagementScoringAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<EngagementScoreDto>>(
            $"v1/analytics/culture/engagementscoring?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Trend Modeling section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<CultureTrendModelDto>>> GetTrendModelingAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<CultureTrendModelDto>>(
            $"v1/analytics/culture/trendmodeling?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
