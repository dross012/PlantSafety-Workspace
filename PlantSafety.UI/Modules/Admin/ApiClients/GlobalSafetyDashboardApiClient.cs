// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Typed API client for the Admin/Executive Global Safety Dashboard (Phase 8
//   §5 Multi-Site Safety Intelligence Network). Wraps all HTTP communication for
//   PlantSafety.Api.GlobalSafetyInsights.Controllers.V1.GlobalSafetyDashboardV1Controller.
//   Every request is tenant-scoped server-side (TenantId resolved from the caller's operation
//   context, honoring any active impersonation session) -- no tenantId parameter is ever sent
//   from the client.
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
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>Typed API client for the Admin/Executive Global Safety Dashboard.</summary>
public sealed class GlobalSafetyDashboardApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="GlobalSafetyDashboardApiClient"/>.</summary>
    public GlobalSafetyDashboardApiClient(HttpClient http, ILogger<GlobalSafetyDashboardApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the cross-site KPI snapshot summary plus its computation history for the caller's operating tenant.</summary>
    public Task<ApiResponseEnvelope<GlobalSafetyInsightsQueryResultDto>> GetSummaryAsync(
        DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken cancellationToken = default)
        => GetAsync<GlobalSafetyInsightsQueryResultDto>(
            $"v1/global-safety-dashboard/summary{BuildRangeQuery(fromUtc, toUtc)}", cancellationToken);

    /// <summary>Retrieves the full detail (per-site risk scores, anomaly detail, recommendation detail) of the latest snapshot.</summary>
    public Task<ApiResponseEnvelope<GlobalSafetyInsightsDetailDto>> GetDetailAsync(CancellationToken cancellationToken = default)
        => GetAsync<GlobalSafetyInsightsDetailDto>("v1/global-safety-dashboard/detail", cancellationToken);

    /// <summary>Lists cross-site coordinated-action audit trail entries for the caller's operating tenant.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<CrossSiteActionLogEntryDto>>> ListCrossSiteActionsAsync(
        DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<CrossSiteActionLogEntryDto>>(
            $"v1/global-safety-dashboard/cross-site-actions{BuildRangeQuery(fromUtc, toUtc)}", cancellationToken);

    private static string BuildRangeQuery(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc is null && toUtc is null)
            return string.Empty;

        var parts = new List<string>(2);
        if (fromUtc is { } from)
            parts.Add($"fromUtc={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}");
        if (toUtc is { } to)
            parts.Add($"toUtc={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}");

        return "?" + string.Join("&", parts);
    }
}
