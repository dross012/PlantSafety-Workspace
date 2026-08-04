// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the HazardousWasteManagement module. Wraps HTTP
//   communication with the real PlantSafety.Api read-model endpoints (GET
//   api/v1/hazardouswastemanagement/manifests/open and
//   api/v1/hazardouswastemanagement/accumulation-areas/nearing-time-limit), resolved via
//   ListOpenManifestsQuery / ListAccumulationAreasNearingTimeLimitQuery /
//   HazardousWasteManagementV1Controller. Unwraps the server's ApiResponse&lt;T&gt; envelope into
//   the UI's own ApiResponseEnvelope&lt;T&gt;. No HttpClient calls are permitted outside this class
//   for the HazardousWasteManagement module.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.HazardousWasteManagement.Models;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.ApiClients;

/// <summary>
/// Typed API client for the HazardousWasteManagement module.
/// Components MUST inject this class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class HazardousWasteManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="HazardousWasteManagementApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public HazardousWasteManagementApiClient(HttpClient http, ILogger<HazardousWasteManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves all manifests that are not yet Closed, feeding the Hazardous Waste Lifecycle Tracker dashboard.</summary>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task<ApiResponseEnvelope<ListOpenManifestsResponseDto>> ListOpenManifestsAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var uri = "v1/hazardouswastemanagement/manifests/open" +
                  (siteId.HasValue ? $"?SiteId={siteId.Value:D}" : string.Empty);

        var envelope = await GetAsync<ServerEnvelope<ListOpenManifestsResponseDto>>(uri, cancellationToken)
            .ConfigureAwait(false);

        return Unwrap(envelope);
    }

    /// <summary>Retrieves accumulation areas with containers approaching their regulatory time limit,
    /// feeding the Accumulation Area Time-Limit Board dashboard.</summary>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task<ApiResponseEnvelope<ListAccumulationAreasNearingTimeLimitResponseDto>> ListAccumulationAreasNearingTimeLimitAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var uri = "v1/hazardouswastemanagement/accumulation-areas/nearing-time-limit" +
                  (siteId.HasValue ? $"?SiteId={siteId.Value:D}" : string.Empty);

        var envelope = await GetAsync<ServerEnvelope<ListAccumulationAreasNearingTimeLimitResponseDto>>(uri, cancellationToken)
            .ConfigureAwait(false);

        return Unwrap(envelope);
    }

    /// <summary>
    /// Unwraps the server's <c>ApiResponse&lt;T&gt;</c> envelope (Success/Data/Errors) into the
    /// UI's discriminated <see cref="ApiResponseEnvelope{T}"/> shape.
    /// </summary>
    private static ApiResponseEnvelope<T> Unwrap<T>(ApiResponseEnvelope<ServerEnvelope<T>> envelope)
    {
        if (envelope.IsSuccess)
        {
            var server = envelope.Data!;
            return server.Success && server.Data is not null
                ? ApiResponseEnvelope<T>.Success(server.Data)
                : ApiResponseEnvelope<T>.FromProblem(new ProblemDetailsDto
                {
                    Title = "Request failed",
                    Detail = server.Errors.Count > 0
                        ? string.Join(" ", server.Errors)
                        : "The server reported failure without an error detail.",
                    Status = 400,
                });
        }

        return envelope.IsProblem
            ? ApiResponseEnvelope<T>.FromProblem(envelope.Problem!)
            : ApiResponseEnvelope<T>.Fault(envelope.FaultType ?? "Unknown", envelope.FaultMessage ?? string.Empty);
    }

    /// <summary>
    /// Mirrors the server-side <c>PlantSafety.Api.Models.ApiResponse&lt;T&gt;</c> envelope shape
    /// for deserialization only. Metadata is intentionally omitted — the UI does not need it.
    /// </summary>
    private sealed class ServerEnvelope<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];
    }
}
