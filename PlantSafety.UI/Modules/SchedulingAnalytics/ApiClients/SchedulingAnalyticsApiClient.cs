// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Scheduling Analytics module.
//   Wraps HTTP communication for the single Scheduling Analytics read-model
//   endpoint exposed by PlantSafety.Api.Analytics.Schedules:
//     GET api/v1/analytics/schedules
//   Shares the {TenantId, SiteId?, From, To?} query contract and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;) with every other Analytics
//   section endpoint. No HttpClient calls are permitted outside this class
//   for the Scheduling Analytics module.
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
using PlantSafety.UI.Modules.SchedulingAnalytics.Models;

namespace PlantSafety.UI.Modules.SchedulingAnalytics.ApiClients;

/// <summary>
/// Typed API client for the Scheduling Analytics module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class SchedulingAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="SchedulingAnalyticsApiClient"/>.</summary>
    public SchedulingAnalyticsApiClient(HttpClient http, ILogger<SchedulingAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the schedule entries for the tenant and optional site.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<ScheduleEntryDto>>> GetSchedulesAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<ScheduleEntryDto>>(
            $"v1/analytics/schedules?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
