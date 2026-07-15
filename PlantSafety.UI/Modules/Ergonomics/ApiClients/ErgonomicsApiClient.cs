// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Ergonomics module. Wraps HTTP
//   communication for GET /v1/operational/ergonomics (tenant/site/date-range
//   scoped read-model). No HttpClient calls are permitted outside this class
//   for the Ergonomics module.
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
using PlantSafety.UI.Modules.Ergonomics.Models;

namespace PlantSafety.UI.Modules.Ergonomics.ApiClients;

/// <summary>Typed API client for the Ergonomics module.</summary>
public sealed class ErgonomicsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ErgonomicsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public ErgonomicsApiClient(HttpClient http, ILogger<ErgonomicsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves ergonomics assessment records for the given tenant/site within an inclusive UTC date range.</summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="fromUtc">Inclusive start of the date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<ErgonomicsResponseDto>> GetErgonomicsAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var query = $"v1/operational/ergonomics?tenantId={tenantId}&from={fromUtc:O}&to={toUtc:O}";
        if (siteId is not null) query += $"&siteId={siteId}";

        return GetAsync<ErgonomicsResponseDto>(query, cancellationToken);
    }
}
