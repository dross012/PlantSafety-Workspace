// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Dashboard Analytics module.
//   Wraps HTTP communication for the four Dashboard Analytics read-model
//   sections exposed by PlantSafety.Api.Analytics.Dashboard.*:
//     GET api/v1/analytics/dashboard/executive
//     GET api/v1/analytics/dashboard/kpisnapshots
//     GET api/v1/analytics/dashboard/readmodel
//     GET api/v1/analytics/dashboard/trendanalysis
//   All four endpoints share the {TenantId, SiteId?, From, To?} query
//   contract (see GetExecutiveRequest et al.) and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;). No HttpClient calls are
//   permitted outside this class for the Dashboard Analytics module.
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
using PlantSafety.UI.Modules.DashboardAnalytics.Models;

namespace PlantSafety.UI.Modules.DashboardAnalytics.ApiClients;

/// <summary>
/// Typed API client for the Dashboard Analytics module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class DashboardAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="DashboardAnalyticsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public DashboardAnalyticsApiClient(HttpClient http, ILogger<DashboardAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the Executive summary metrics section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<ExecutiveMetricDto>>> GetExecutiveAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<ExecutiveMetricDto>>(
            $"v1/analytics/dashboard/executive?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the KPI Snapshots section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<KpiSnapshotDto>>> GetKpiSnapshotsAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<KpiSnapshotDto>>(
            $"v1/analytics/dashboard/kpisnapshots?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the generic Read Model widget rows section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<DashboardWidgetRowDto>>> GetReadModelAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<DashboardWidgetRowDto>>(
            $"v1/analytics/dashboard/readmodel?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the dashboard-scoped Trend Analysis section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<DashboardTrendPointDto>>> GetTrendAnalysisAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<DashboardTrendPointDto>>(
            $"v1/analytics/dashboard/trendanalysis?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>
    /// Builds the <c>TenantId</c>/<c>SiteId</c>/<c>From</c>/<c>To</c> query string shared by every
    /// Analytics section read-model endpoint (see e.g. GetExecutiveRequest).
    /// </summary>
    internal static string BuildQuery(Guid tenantId, Guid? siteId, DateOnly from, DateOnly to)
    {
        var tenantSegment = Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture));
        var fromSegment = Uri.EscapeDataString(from.ToDateTime(TimeOnly.MinValue).ToString("O", CultureInfo.InvariantCulture));
        var toSegment = Uri.EscapeDataString(to.ToDateTime(TimeOnly.MaxValue).ToString("O", CultureInfo.InvariantCulture));

        var query = $"TenantId={tenantSegment}&From={fromSegment}&To={toSegment}";

        if (siteId is { } site)
        {
            query += $"&SiteId={Uri.EscapeDataString(site.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return query;
    }
}
