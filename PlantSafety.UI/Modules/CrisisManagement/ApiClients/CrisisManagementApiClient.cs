// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Crisis Management module. Wraps
//   HTTP communication for declaring crises (POST) and for the List/GetById
//   read endpoints under /v1/features/safety-events/crisis-events, backed by
//   PlantSafety.Api.Features.SafetyEvents.CrisisManagement.Controllers.V1
//   .CrisisEventsV1Controller.
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
using PlantSafety.UI.Modules.CrisisManagement.Models;

namespace PlantSafety.UI.Modules.CrisisManagement.ApiClients;

/// <summary>Typed API client for the Crisis Management module.</summary>
public sealed class CrisisManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="CrisisManagementApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CrisisManagementApiClient(HttpClient http, ILogger<CrisisManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated list of declared crisis events.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<CrisisEventDto>>> ListCrisisEventsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<CrisisEventDto>>(
            $"v1/features/safety-events/crisis-events?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>Retrieves a single crisis event by its identifier.</summary>
    /// <param name="crisisEventId">The unique crisis event identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<CrisisEventDto>> GetCrisisEventAsync(
        Guid crisisEventId,
        CancellationToken cancellationToken = default)
        => GetAsync<CrisisEventDto>($"v1/features/safety-events/crisis-events/{crisisEventId}", cancellationToken);
}
