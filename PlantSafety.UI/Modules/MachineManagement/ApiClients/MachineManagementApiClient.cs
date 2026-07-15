// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Machine Management module. Wraps
//   all HTTP communication against the CrossDomain Machines read-model
//   (api/v1/cross-domain/machines). All methods return
//   ApiResponseEnvelope<T>. No HttpClient calls are permitted outside this
//   class for the Machine Management module.
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
using PlantSafety.UI.Modules.MachineManagement.Models;

namespace PlantSafety.UI.Modules.MachineManagement.ApiClients;

/// <summary>
/// Typed API client for the Machine Management module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class MachineManagementApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="MachineManagementApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public MachineManagementApiClient(HttpClient http, ILogger<MachineManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves machine/equipment records for the given tenant within the inclusive UTC
    /// date range [<paramref name="from"/>, <paramref name="to"/>].
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the read-model response or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<MachineItemDto>>> GetMachinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<MachineItemDto>>(
            $"v1/cross-domain/machines?{BuildReadModelQuery(tenantId, siteId, from, to)}",
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
