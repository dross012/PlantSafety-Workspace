// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Sustainability module. Targets
//   the not-yet-wired GET v1/sustainability/metrics endpoint (no
//   PlantSafety.Api controller exists for this module today). Calls
//   gracefully fault through ApiResponseEnvelope so the UI degrades to its
//   Error state rather than blocking on the missing backend.
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
using PlantSafety.UI.Modules.Sustainability.Models;

namespace PlantSafety.UI.Modules.Sustainability.ApiClients;

/// <summary>Typed API client for the Sustainability module.</summary>
public sealed class SustainabilityApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="SustainabilityApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public SustainabilityApiClient(HttpClient http, ILogger<SustainabilityApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves sustainability metrics for the given tenant within the requested window.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="from">Inclusive start of the reporting window (UTC).</param>
    /// <param name="to">Inclusive end of the reporting window (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the metric list, or a problem/fault while the backend is unwired.</returns>
    public Task<ApiResponseEnvelope<SustainabilityResponseDto>> GetMetricsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                    $"&From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<SustainabilityResponseDto>($"v1/sustainability/metrics?{query}", cancellationToken);
    }
}
