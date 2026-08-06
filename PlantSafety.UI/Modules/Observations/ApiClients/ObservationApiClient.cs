// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Observation module. Wraps HTTP
//   communication for the Features/SafetyEvents/Observation endpoints. The
//   backend currently exposes Create/Update/GetById only — no list endpoint
//   exists yet (PlantSafety.Api.Features.SafetyEvents.Observation.Controllers.V1
//   .ObservationsV1Controller has no bare-collection GET action). ListObservationsAsync
//   is defined against the conventional collection route so the list page degrades
//   gracefully (via ApiResponseEnvelope.IsProblem/IsFault) until that endpoint ships,
//   and starts working automatically once it does — no UI change required.
//   CreateObservationAsync/UpdateObservationAsync wrap the real, wired
//   CreateObservationCommand/UpdateObservationCommand.
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
using PlantSafety.UI.Modules.Observations.Models;

namespace PlantSafety.UI.Modules.Observations.ApiClients;

/// <summary>Typed API client for the Observation module.</summary>
public sealed class ObservationApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ObservationApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public ObservationApiClient(HttpClient http, ILogger<ObservationApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of observation records. No backend list endpoint exists yet —
    /// this call is expected to surface a graceful <see cref="ApiResponseEnvelope{T}.IsProblem"/>
    /// or <see cref="ApiResponseEnvelope{T}.IsFault"/> result until one is deployed.
    /// </summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<ObservationDto>>> ListObservationsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<ObservationDto>>(
            $"v1/features/safety-events/observations?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>Retrieves the full projection for a single observation by its identifier.</summary>
    /// <param name="observationId">The unique observation identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<ObservationDto>> GetObservationAsync(
        Guid observationId,
        CancellationToken cancellationToken = default)
        => GetAsync<ObservationDto>($"v1/features/safety-events/observations/{observationId}", cancellationToken);

    /// <summary>Creates a new Safety Observation. Backed by POST <c>v1/features/safety-events/observations</c> (CreateObservationCommand).</summary>
    /// <param name="request">The observation fields to submit.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<Guid>> CreateObservationAsync(
        CreateObservationRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateObservationRequest, Guid>("v1/features/safety-events/observations", request, cancellationToken);

    /// <summary>Updates mutable fields of an existing Safety Observation. Backed by PUT <c>v1/features/safety-events/observations/{id}</c> (UpdateObservationCommand).</summary>
    /// <param name="observationId">The observation to update.</param>
    /// <param name="request">The updated fields.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<Guid>> UpdateObservationAsync(
        Guid observationId,
        UpdateObservationRequest request,
        CancellationToken cancellationToken = default)
        => PutAsync<UpdateObservationRequest, Guid>($"v1/features/safety-events/observations/{observationId}", request, cancellationToken);
}
