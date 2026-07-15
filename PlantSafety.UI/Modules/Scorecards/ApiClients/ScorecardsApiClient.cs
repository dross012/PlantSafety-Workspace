// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Scorecards module. Wraps HTTP
//   communication for the four Scorecards read-model sections exposed by
//   PlantSafety.Api.Analytics.ScoreCards.*:
//     GET api/v1/analytics/scorecards/compliance
//     GET api/v1/analytics/scorecards/department
//     GET api/v1/analytics/scorecards/supervisor
//     GET api/v1/analytics/scorecards/training
//   Shares the {TenantId, SiteId?, From, To?} query contract and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;) with every other Analytics
//   section endpoint. No HttpClient calls are permitted outside this class
//   for the Scorecards module.
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
using PlantSafety.UI.Modules.Scorecards.Models;

namespace PlantSafety.UI.Modules.Scorecards.ApiClients;

/// <summary>
/// Typed API client for the Scorecards module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class ScorecardsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ScorecardsApiClient"/>.</summary>
    public ScorecardsApiClient(HttpClient http, ILogger<ScorecardsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the Compliance scorecard section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<ComplianceScorecardDto>>> GetComplianceAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<ComplianceScorecardDto>>(
            $"v1/analytics/scorecards/compliance?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Department scorecard section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<DepartmentScorecardDto>>> GetDepartmentAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<DepartmentScorecardDto>>(
            $"v1/analytics/scorecards/department?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Supervisor scorecard section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<SupervisorScorecardDto>>> GetSupervisorAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<SupervisorScorecardDto>>(
            $"v1/analytics/scorecards/supervisor?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Training scorecard section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<TrainingScorecardDto>>> GetTrainingAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<TrainingScorecardDto>>(
            $"v1/analytics/scorecards/training?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
