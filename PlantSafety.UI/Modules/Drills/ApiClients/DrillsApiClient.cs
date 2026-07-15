// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Drills & Exercises module. Wraps
//   HTTP communication for scheduling drills (POST) and for the List/GetById
//   read endpoints under /v1/features/safety-events/drills, backed by
//   PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Controllers.V1
//   .DrillsV1Controller.
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
using PlantSafety.UI.Modules.Drills.Models;

namespace PlantSafety.UI.Modules.Drills.ApiClients;

/// <summary>Typed API client for the Drills &amp; Exercises module.</summary>
public sealed class DrillsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="DrillsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public DrillsApiClient(HttpClient http, ILogger<DrillsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated list of drills.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<DrillDto>>> ListDrillsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<DrillDto>>(
            $"v1/features/safety-events/drills?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>Retrieves a single drill by its identifier.</summary>
    /// <param name="drillId">The unique drill identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<DrillDto>> GetDrillAsync(
        Guid drillId,
        CancellationToken cancellationToken = default)
        => GetAsync<DrillDto>($"v1/features/safety-events/drills/{drillId}", cancellationToken);
}
