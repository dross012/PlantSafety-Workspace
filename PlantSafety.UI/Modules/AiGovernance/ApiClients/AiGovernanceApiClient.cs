// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the AI Governance module. Targets the
//   not-yet-wired GET v1/ai-governance/models endpoint (no PlantSafety.Api
//   controller exists for this module today). Calls gracefully fault through
//   ApiResponseEnvelope so the UI degrades to its Error state rather than
//   blocking on the missing backend.
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
using PlantSafety.UI.Modules.AiGovernance.Models;

namespace PlantSafety.UI.Modules.AiGovernance.ApiClients;

/// <summary>Typed API client for the AI Governance module.</summary>
public sealed class AiGovernanceApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="AiGovernanceApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AiGovernanceApiClient(HttpClient http, ILogger<AiGovernanceApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the governed AI/ML model registry for the given tenant.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the governed model list, or a problem/fault while the backend is unwired.</returns>
    public Task<ApiResponseEnvelope<AiGovernanceResponseDto>> GetGovernedModelsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<AiGovernanceResponseDto>($"v1/ai-governance/models?{query}", cancellationToken);
    }
}
