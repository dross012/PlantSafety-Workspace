// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Enterprise Fabric module. Targets
//   the not-yet-wired GET v1/enterprise-fabric/nodes endpoint (no
//   PlantSafety.Api read-model controller exists for fabric topology today).
//   Calls gracefully fault through ApiResponseEnvelope so the UI degrades to
//   its Error state rather than blocking on the missing backend.
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
using PlantSafety.UI.Modules.EnterpriseFabric.Models;

namespace PlantSafety.UI.Modules.EnterpriseFabric.ApiClients;

/// <summary>Typed API client for the Enterprise Fabric module.</summary>
public sealed class EnterpriseFabricApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="EnterpriseFabricApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public EnterpriseFabricApiClient(HttpClient http, ILogger<EnterpriseFabricApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the integration fabric node registry for the given tenant.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the fabric node list, or a problem/fault while the backend is unwired.</returns>
    public Task<ApiResponseEnvelope<EnterpriseFabricResponseDto>> GetFabricNodesAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<EnterpriseFabricResponseDto>($"v1/enterprise-fabric/nodes?{query}", cancellationToken);
    }
}
