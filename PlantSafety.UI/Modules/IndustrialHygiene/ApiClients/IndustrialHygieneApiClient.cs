// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Industrial Hygiene module.
//   Wraps HTTP communication for GET /v1/operational/industrial-hygiene
//   (tenant/site/date-range scoped read-model). No HttpClient calls are
//   permitted outside this class for the Industrial Hygiene module.
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
using PlantSafety.UI.Modules.IndustrialHygiene.Models;

namespace PlantSafety.UI.Modules.IndustrialHygiene.ApiClients;

/// <summary>Typed API client for the Industrial Hygiene module.</summary>
public sealed class IndustrialHygieneApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="IndustrialHygieneApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public IndustrialHygieneApiClient(HttpClient http, ILogger<IndustrialHygieneApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves industrial hygiene sample records for the given tenant/site within an inclusive UTC date range.</summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="fromUtc">Inclusive start of the date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<IndustrialHygieneResponseDto>> GetIndustrialHygieneSamplesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var query = $"v1/operational/industrial-hygiene?tenantId={tenantId}&from={fromUtc:O}&to={toUtc:O}";
        if (siteId is not null) query += $"&siteId={siteId}";

        return GetAsync<IndustrialHygieneResponseDto>(query, cancellationToken);
    }
}
