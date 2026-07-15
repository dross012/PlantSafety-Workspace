// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — PlantSafety.Domain/Application/Api/Infrastructure now own a
//   real WorkerWellBeing Features/ CQRS slice (List/GetById), backed by
//   [WorkerWellBeing].[WorkerWellBeingRecords]. These calls target the real
//   v1/human-factors/worker-wellbeing routes; ApiResponseEnvelope.IsProblem/IsFault
//   still degrades gracefully on transient failures.
// FILE DESCRIPTION: Typed API client for the Worker WellBeing module.
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
using PlantSafety.UI.Modules.WorkerWellBeing.Models;

namespace PlantSafety.UI.Modules.WorkerWellBeing.ApiClients;

/// <summary>Typed API client for the Worker WellBeing module.</summary>
public sealed class WorkerWellBeingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="WorkerWellBeingApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public WorkerWellBeingApiClient(HttpClient http, ILogger<WorkerWellBeingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated list of Worker WellBeing records.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<WorkerWellBeingDto>>> ListWorkerWellBeingRecordsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<WorkerWellBeingDto>>(
            $"v1/human-factors/worker-wellbeing?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>Retrieves a single Worker WellBeing record by identifier.</summary>
    /// <param name="workerWellBeingId">The unique record identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<WorkerWellBeingDto>> GetWorkerWellBeingRecordAsync(
        Guid workerWellBeingId,
        CancellationToken cancellationToken = default)
        => GetAsync<WorkerWellBeingDto>($"v1/human-factors/worker-wellbeing/{workerWellBeingId}", cancellationToken);
}
