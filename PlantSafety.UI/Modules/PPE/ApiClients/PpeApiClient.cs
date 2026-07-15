// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the PPE module. Wraps all HTTP
//   communication against the Operational PPEManagement read-model
//   (api/v1/operational/ppe-management). All methods return
//   ApiResponseEnvelope<T>. No HttpClient calls are permitted outside this
//   class for the PPE module.
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
using PlantSafety.UI.Modules.PPE.Models;

namespace PlantSafety.UI.Modules.PPE.ApiClients;

/// <summary>
/// Typed API client for the PPE module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class PpeApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="PpeApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public PpeApiClient(HttpClient http, ILogger<PpeApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves PPE issuance/compliance records for the given tenant within the inclusive
    /// UTC date range [<paramref name="from"/>, <paramref name="to"/>].
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the read-model response or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<PpeItemDto>>> GetPpeRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<PpeItemDto>>(
            $"v1/operational/ppe-management?{BuildReadModelQuery(tenantId, siteId, from, to)}",
            cancellationToken);

    /// <summary>
    /// Builds the canonical TenantId/SiteId/From/To query string shared by every
    /// PlantSafety.Api read-model endpoint (Compliance/Operational/CrossDomain/Analytics).
    /// </summary>
    private static string BuildReadModelQuery(Guid tenantId, Guid? siteId, DateTime from, DateTime to)
    {
        var query = $"TenantId={tenantId:D}&From={Uri.EscapeDataString(from.ToString("o"))}&To={Uri.EscapeDataString(to.ToString("o"))}";
        return siteId.HasValue ? $"{query}&SiteId={siteId.Value:D}" : query;
    }
}
