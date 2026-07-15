// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the AI Investigation module. Wraps HTTP
//   communication with the real PlantSafety.Api read-model endpoint at
//   GET v1/intelligence/allinvestigations (TenantId/SiteId/From/To query params).
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
using PlantSafety.UI.Modules.AiInvestigation.Models;

namespace PlantSafety.UI.Modules.AiInvestigation.ApiClients;

/// <summary>Typed API client for the AI Investigation module.</summary>
public sealed class AiInvestigationApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="AiInvestigationApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AiInvestigationApiClient(HttpClient http, ILogger<AiInvestigationApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves AI-assisted investigation records for the given tenant within the requested window.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the investigation list or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<AiInvestigationResponseDto>> GetInvestigationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => GetAsync<AiInvestigationResponseDto>(
            $"v1/intelligence/allinvestigations?{BuildQuery(tenantId, siteId, from, to)}",
            cancellationToken);

    private static string BuildQuery(Guid tenantId, Guid? siteId, DateTime from, DateTime to)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                     $"&From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                     $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return query;
    }
}
