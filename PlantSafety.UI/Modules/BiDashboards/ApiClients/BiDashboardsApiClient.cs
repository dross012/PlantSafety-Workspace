// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-02
// FILE DESCRIPTION: Typed API client for the Phase 5 §4.3 BI Dashboards
//   sub-features. Wraps HTTP communication for the six new
//   PlantSafety.Application.Analytics.Dashboard.*Dashboard read-model
//   queries (GetSchedulingDashboardQuery/GetProductionDashboardQuery/
//   GetMaintenanceDashboardQuery/GetRiskDashboardQuery/
//   GetContinuousImprovementDashboardQuery/GetAuditDashboardQuery):
//     GET api/v1/analytics/dashboard/scheduling
//     GET api/v1/analytics/dashboard/production
//     GET api/v1/analytics/dashboard/maintenance
//     GET api/v1/analytics/dashboard/risk
//     GET api/v1/analytics/dashboard/continuousimprovement
//     GET api/v1/analytics/dashboard/audit
//   All six endpoints share the {TenantId, SiteId?, From, To} query contract
//   used by every Analytics.Dashboard sub-feature (see
//   DashboardAnalyticsApiClient.BuildQuery) and return an
//   {IReadOnlyList&lt;TItem&gt;} result set wrapped in the platform-wide
//   ApiResponse&lt;T&gt; envelope. No HttpClient calls are permitted outside
//   this class for the BI Dashboards module.
//
//   NOTE: as of this writing, PlantSafety.Api only exposes dedicated V1
//   controllers for the four original Dashboard sub-features (Executive,
//   KPISnapshots, ReadModel, TrendAnalysis) — see
//   PlantSafety.Api/Analytics/Dashboard/&lt;SubFeature&gt;/Controllers/V1. The six
//   Application-layer queries this client targets do not yet have API
//   controllers wired up; this client is built ahead of that Api-layer work
//   following the exact routing/response convention of the existing
//   sub-features so it activates with zero UI changes once those controllers
//   are added.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.BiDashboards.Models;

namespace PlantSafety.UI.Modules.BiDashboards.ApiClients;

/// <summary>
/// Typed API client for the BI Dashboards module (Phase 5 §4.3).
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class BiDashboardsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="BiDashboardsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public BiDashboardsApiClient(HttpClient http, ILogger<BiDashboardsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the Scheduling BI dashboard KPI section.</summary>
    public Task<ApiResponseEnvelope<BiDashboardSectionDto<SchedulingDashboardItemDto>>> GetSchedulingAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<BiDashboardSectionDto<SchedulingDashboardItemDto>>(
            $"v1/analytics/dashboard/scheduling?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Production/Operational BI dashboard KPI section.</summary>
    public Task<ApiResponseEnvelope<BiDashboardSectionDto<ProductionDashboardItemDto>>> GetProductionAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<BiDashboardSectionDto<ProductionDashboardItemDto>>(
            $"v1/analytics/dashboard/production?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Maintenance BI dashboard KPI section.</summary>
    public Task<ApiResponseEnvelope<BiDashboardSectionDto<MaintenanceDashboardItemDto>>> GetMaintenanceAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<BiDashboardSectionDto<MaintenanceDashboardItemDto>>(
            $"v1/analytics/dashboard/maintenance?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Risk BI dashboard KPI section.</summary>
    public Task<ApiResponseEnvelope<BiDashboardSectionDto<RiskDashboardItemDto>>> GetRiskAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<BiDashboardSectionDto<RiskDashboardItemDto>>(
            $"v1/analytics/dashboard/risk?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the ContinuousImprovement BI dashboard KPI section.</summary>
    public Task<ApiResponseEnvelope<BiDashboardSectionDto<ContinuousImprovementDashboardItemDto>>> GetContinuousImprovementAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<BiDashboardSectionDto<ContinuousImprovementDashboardItemDto>>(
            $"v1/analytics/dashboard/continuousimprovement?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Fetches the Audit BI dashboard KPI section.</summary>
    public Task<ApiResponseEnvelope<BiDashboardSectionDto<AuditDashboardItemDto>>> GetAuditAsync(
        Guid tenantId, Guid? siteId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => GetAsync<BiDashboardSectionDto<AuditDashboardItemDto>>(
            $"v1/analytics/dashboard/audit?{BuildQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>
    /// Builds the <c>TenantId</c>/<c>SiteId</c>/<c>From</c>/<c>To</c> query string shared by every
    /// Analytics.Dashboard sub-feature read-model endpoint.
    /// </summary>
    private static string BuildQuery(Guid tenantId, Guid? siteId, DateOnly from, DateOnly to)
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

/// <summary>
/// Generic response envelope for a single BI Dashboard sub-feature section, matching the
/// {TenantId, SiteId, GeneratedAt, Items} shape shared by every Analytics.Dashboard
/// sub-feature (see AnalyticsSectionResponseDto&lt;TItem&gt;).
/// </summary>
/// <typeparam name="TItem">The item type contained in <see cref="Items"/>.</typeparam>
public sealed class BiDashboardSectionDto<TItem>
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the result items. Never null; empty when no results.</summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];
}
