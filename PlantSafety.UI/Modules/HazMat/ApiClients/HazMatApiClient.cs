// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the HazMat module. Wraps HTTP
//   communication with the real PlantSafety.Api read-model endpoint
//   (GET api/v1/operational/hazardous-materials, resolved via
//   GetHazardousMaterialsQuery / HazardousMaterialsV1Controller). The server
//   wraps every response in the canonical PlantSafety.Api.Models.
//   ApiResponse&lt;T&gt; envelope (Success/Data/Errors/Metadata); this client
//   unwraps that shape into the UI's own ApiResponseEnvelope&lt;T&gt; so callers
//   use the same discriminated Success/Problem/Fault pattern as every other
//   module. No HttpClient calls are permitted outside this class for the
//   HazMat module.
// SchemaVersion: 2026.1
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
using PlantSafety.UI.Modules.HazMat.Models;

namespace PlantSafety.UI.Modules.HazMat.ApiClients;

/// <summary>
/// Typed API client for the HazMat module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class HazMatApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="HazMatApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public HazMatApiClient(HttpClient http, ILogger<HazMatApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves HazMat read-model records for the given tenant within an inclusive
    /// UTC date range, optionally scoped to a single site.
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="fromUtc">Inclusive start of the date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the HazMat result set or a problem/fault.</returns>
    public async Task<ApiResponseEnvelope<HazMatResponseDto>> GetHazardousMaterialsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var uri = $"v1/operational/hazardous-materials?TenantId={tenantId:D}" +
                  (siteId.HasValue ? $"&SiteId={siteId.Value:D}" : string.Empty) +
                  $"&From={Uri.EscapeDataString(fromUtc.ToString("O"))}" +
                  $"&To={Uri.EscapeDataString(toUtc.ToString("O"))}";

        var envelope = await GetAsync<ServerEnvelope<HazMatResponseDto>>(uri, cancellationToken)
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
