// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Reporting Analytics module.
//   Wraps HTTP communication for the single Reporting Analytics read-model
//   endpoint exposed by PlantSafety.Api.Analytics.Reporting:
//     GET api/v1/analytics/reporting
//   Shares the {TenantId, SiteId?, From, To?} query contract and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;) with every other Analytics
//   section endpoint. No HttpClient calls are permitted outside this class
//   for the Reporting Analytics module.
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
using PlantSafety.UI.Modules.ReportingAnalytics.Models;

namespace PlantSafety.UI.Modules.ReportingAnalytics.ApiClients;

/// <summary>
/// Typed API client for the Reporting Analytics module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class ReportingAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ReportingAnalyticsApiClient"/>.</summary>
    public ReportingAnalyticsApiClient(HttpClient http, ILogger<ReportingAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the generated report entries for the tenant and optional site.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<ReportEntryDto>>> GetReportsAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<ReportEntryDto>>(
            $"v1/analytics/reporting?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
