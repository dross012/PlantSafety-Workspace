// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — repointed at the new dedicated Environmental Health
//   monitoring-reading endpoint (GET v1/features/environmental-health/readings
//   and GET v1/features/environmental-health/readings/{id}) now that
//   PlantSafety.Domain/Application/Api/Infrastructure own a real
//   EnvironmentalHealth aggregate. Previously this client borrowed the
//   Compliance/Policy/Environmental read-model as a stand-in.
// FILE DESCRIPTION: Typed API client for the Environmental Health module.
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
using PlantSafety.UI.Modules.EnvironmentalHealth.Models;

namespace PlantSafety.UI.Modules.EnvironmentalHealth.ApiClients;

/// <summary>Typed API client for the Environmental Health module.</summary>
public sealed class EnvironmentalHealthApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="EnvironmentalHealthApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public EnvironmentalHealthApiClient(HttpClient http, ILogger<EnvironmentalHealthApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated list of Environmental Health readings within the requested window.</summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the environmental health reading list or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<EnvironmentalHealthResponseDto>> ListReadingsAsync(
        Guid? siteId,
        DateTime from,
        DateTime to,
        int pageNumber = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
        => GetAsync<EnvironmentalHealthResponseDto>(
            $"v1/features/environmental-health/readings?{BuildQuery(siteId, from, to, pageNumber, pageSize)}",
            cancellationToken);

    /// <summary>Retrieves a single Environmental Health reading by identifier.</summary>
    /// <param name="environmentalHealthId">The reading identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<EnvironmentalHealthItemDto>> GetReadingByIdAsync(
        Guid environmentalHealthId,
        CancellationToken cancellationToken = default)
        => GetAsync<EnvironmentalHealthItemDto>(
            $"v1/features/environmental-health/readings/{environmentalHealthId}",
            cancellationToken);

    private static string BuildQuery(Guid? siteId, DateTime from, DateTime to, int pageNumber, int pageSize)
    {
        var query = $"From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                     $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}" +
                     $"&PageNumber={pageNumber}&PageSize={pageSize}";

        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return query;
    }
}
