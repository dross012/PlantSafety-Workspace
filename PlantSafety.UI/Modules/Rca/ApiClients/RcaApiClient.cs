// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the RCA (Root Cause Analysis) module.
//   The backend currently exposes Create/GetById only — no list endpoint exists
//   yet. ListRcasAsync targets the conventional collection route so the list
//   page degrades gracefully until that endpoint ships.
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
using PlantSafety.UI.Modules.Rca.Models;

namespace PlantSafety.UI.Modules.Rca.ApiClients;

/// <summary>Typed API client for the RCA module.</summary>
public sealed class RcaApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="RcaApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public RcaApiClient(HttpClient http, ILogger<RcaApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of RCA records. No backend list endpoint exists yet — this call
    /// is expected to surface a graceful problem/fault result until one is deployed.
    /// </summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<RootCauseAnalysisDto>>> ListRcasAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<RootCauseAnalysisDto>>(
            $"v1/features/safety-events/root-cause-analysis?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>Retrieves a single RCA record by its identifier.</summary>
    /// <param name="rcaId">The unique RCA identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<RootCauseAnalysisDto>> GetRcaAsync(
        Guid rcaId,
        CancellationToken cancellationToken = default)
        => GetAsync<RootCauseAnalysisDto>($"v1/features/safety-events/root-cause-analysis/{rcaId}", cancellationToken);
}
