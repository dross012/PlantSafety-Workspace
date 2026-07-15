// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Contractor Management module.
//   Wraps HTTP communication for the Contractor read-model list endpoint.
//   All methods return ApiResponseEnvelope<T>. No HttpClient calls are
//   permitted outside this class for the Contractor Management module.
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
using PlantSafety.UI.Modules.ContractorManagement.Models;

namespace PlantSafety.UI.Modules.ContractorManagement.ApiClients;

/// <summary>
/// Typed API client for the Contractor Management module. Components MUST
/// inject this class and call its methods rather than constructing HTTP
/// requests directly.
/// </summary>
public sealed class ContractorManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ContractorManagementApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public ContractorManagementApiClient(HttpClient http, ILogger<ContractorManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves Contractor records for a tenant within a UTC date range. Backed by GET <c>v1/operational/contractor</c>.</summary>
    public Task<ApiResponseEnvelope<ContractorResponseDto>> GetContractorsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                    $"&From={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
        {
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return GetAsync<ContractorResponseDto>($"v1/operational/contractor?{query}", cancellationToken);
    }
}
