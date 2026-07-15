// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Predictive Analytics module.
//   Wraps HTTP communication for the five Predictive Analytics read-model
//   sections exposed by PlantSafety.Api.Analytics.PredictiveAnalytics and
//   PlantSafety.Api.Analytics.Predictive.*:
//     GET api/v1/analytics/predictiveanalytics
//     GET api/v1/analytics/predictive/compliancefailure
//     GET api/v1/analytics/predictive/fatigueriskmodeling
//     GET api/v1/analytics/predictive/incidentprediction
//     GET api/v1/analytics/predictive/riskforecasting
//   Shares the {TenantId, SiteId?, From, To?} query contract and the
//   {TenantId, SiteId, GeneratedAt, Items} response envelope
//   (AnalyticsSectionResponseDto&lt;TItem&gt;) with every other Analytics
//   section endpoint. No HttpClient calls are permitted outside this class
//   for the Predictive Analytics module.
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
using PlantSafety.UI.Modules.PredictiveAnalytics.Models;

namespace PlantSafety.UI.Modules.PredictiveAnalytics.ApiClients;

/// <summary>
/// Typed API client for the Predictive Analytics module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class PredictiveAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="PredictiveAnalyticsApiClient"/>.</summary>
    public PredictiveAnalyticsApiClient(HttpClient http, ILogger<PredictiveAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the top-level Predictive Analytics model output section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<PredictiveModelOutputDto>>> GetModelOutputAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<PredictiveModelOutputDto>>(
            $"v1/analytics/predictiveanalytics?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Compliance Failure prediction section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<ComplianceFailureRiskDto>>> GetComplianceFailureAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<ComplianceFailureRiskDto>>(
            $"v1/analytics/predictive/compliancefailure?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Fatigue Risk Modeling section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<FatigueRiskAssessmentDto>>> GetFatigueRiskModelingAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<FatigueRiskAssessmentDto>>(
            $"v1/analytics/predictive/fatigueriskmodeling?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Incident Prediction section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<IncidentPredictionDto>>> GetIncidentPredictionAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<IncidentPredictionDto>>(
            $"v1/analytics/predictive/incidentprediction?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Risk Forecasting section.</summary>
    public Task<ApiResponseEnvelope<AnalyticsSectionResponseDto<RiskForecastPointDto>>> GetRiskForecastingAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<AnalyticsSectionResponseDto<RiskForecastPointDto>>(
            $"v1/analytics/predictive/riskforecasting?{DashboardAnalyticsApiClient.BuildQuery(tenantId, siteId, from, to)}", cancellationToken);
}
