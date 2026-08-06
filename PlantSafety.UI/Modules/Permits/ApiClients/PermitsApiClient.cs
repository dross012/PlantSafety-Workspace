// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Permits to Work module. Wraps
//   HTTP communication with the real PlantSafety.Api read-model endpoint
//   (GET api/v1/operational/permits-to-work, resolved via
//   GetPermitsToWorkQuery / PermitsToWorkV1Controller). The server wraps
//   every response in the canonical PlantSafety.Api.Models.
//   ApiResponse&lt;T&gt; envelope (Success/Data/Errors/Metadata); this client
//   unwraps that shape into the UI's own ApiResponseEnvelope&lt;T&gt; so callers
//   use the same discriminated Success/Problem/Fault pattern as every other
//   module. No HttpClient calls are permitted outside this class for the
//   Permits to Work module.
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
using PlantSafety.UI.Modules.Permits.Models;

namespace PlantSafety.UI.Modules.Permits.ApiClients;

// ============================================================
// Compliance.Permits write/lifecycle surface (api/v1/compliance/permits) —
// Create -> Approve/Reject -> Complete/Expire -> Close -> Archive, plus a real by-id GET.
// This is a distinct, generic Permit-to-Work aggregate from the read-only Operational.
// PermitsToWork read-model above (no write commands exist there); this is where
// CreatePermitsCommand and its lifecycle actions actually live.
// ============================================================

/// <summary>
/// Typed API client for the Permits to Work module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class PermitsApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="PermitsApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public PermitsApiClient(HttpClient http, ILogger<PermitsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves Permits to Work read-model records for the given tenant within an
    /// inclusive UTC date range, optionally scoped to a single site.
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="fromUtc">Inclusive start of the date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the Permits to Work result set or a problem/fault.</returns>
    public async Task<ApiResponseEnvelope<PermitResponseDto>> GetPermitsToWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var uri = $"v1/operational/permits-to-work?TenantId={tenantId:D}" +
                  (siteId.HasValue ? $"&SiteId={siteId.Value:D}" : string.Empty) +
                  $"&From={Uri.EscapeDataString(fromUtc.ToString("O"))}" +
                  $"&To={Uri.EscapeDataString(toUtc.ToString("O"))}";

        var envelope = await GetAsync<ServerEnvelope<PermitResponseDto>>(uri, cancellationToken)
            .ConfigureAwait(false);

        return Unwrap(envelope);
    }

    /// <summary>Retrieves a single generic Permit record by identity, including its ETag. Backed by GET <c>v1/compliance/permits/{id}</c>.</summary>
    public Task<ApiResponseEnvelope<PermitsItemDto>> GetPermitByIdAsync(
        Guid permitsId, CancellationToken cancellationToken = default)
        => GetAsync<PermitsItemDto>($"v1/compliance/permits/{permitsId}", cancellationToken);

    /// <summary>Submits a new generic Permit record via the real CreatePermitsCommand.</summary>
    public Task<ApiResponseEnvelope<CreatePermitsResultDto>> CreatePermitAsync(
        CreatePermitsRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreatePermitsRequest, CreatePermitsResultDto>("v1/compliance/permits", request, cancellationToken);

    /// <summary>Approves and issues a Permit request (Created -&gt; Active) via ApprovePermitsCommand.</summary>
    public Task<ApiResponseEnvelope<PermitsStatusResultDto>> ApprovePermitAsync(
        Guid permitsId, ApprovePermitsRequest request, CancellationToken cancellationToken = default)
        => PostAsync<ApprovePermitsRequest, PermitsStatusResultDto>(
            $"v1/compliance/permits/{permitsId}/approve", request, cancellationToken);

    /// <summary>Declines a Permit request (Created -&gt; Rejected) via RejectPermitsCommand.</summary>
    public Task<ApiResponseEnvelope<PermitsStatusResultDto>> RejectPermitAsync(
        Guid permitsId, RejectPermitsRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RejectPermitsRequest, PermitsStatusResultDto>(
            $"v1/compliance/permits/{permitsId}/reject", request, cancellationToken);

    /// <summary>Marks the permitted work completed (Active -&gt; Completed) via CompletePermitsCommand.</summary>
    public Task<ApiResponseEnvelope<PermitsStatusResultDto>> CompletePermitAsync(
        Guid permitsId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PermitsStatusResultDto>(
            $"v1/compliance/permits/{permitsId}/complete", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Expires an issued permit that passed its validity window (Active -&gt; Expired) via ExpirePermitsCommand.</summary>
    public Task<ApiResponseEnvelope<PermitsStatusResultDto>> ExpirePermitAsync(
        Guid permitsId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PermitsStatusResultDto>(
            $"v1/compliance/permits/{permitsId}/expire", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Formally closes a completed permit (Completed -&gt; Closed) via ClosePermitsCommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<PermitsStatusResultDto>> ClosePermitAsync(
        Guid permitsId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PermitsStatusResultDto>(
            $"v1/compliance/permits/{permitsId}/close", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Archives a closed permit for compliance retention (Closed -&gt; Archived) via ArchivePermitsCommand.</summary>
    public Task<ApiResponseEnvelope<PermitsStatusResultDto>> ArchivePermitAsync(
        Guid permitsId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PermitsStatusResultDto>(
            $"v1/compliance/permits/{permitsId}/archive", new EtagRequest { ETag = eTag }, cancellationToken);

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
