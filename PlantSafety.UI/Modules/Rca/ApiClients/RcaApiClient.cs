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

    /// <summary>Initiates a new RCA linked to an Incident/NearMiss via the real CreateRootCauseAnalysisCommand.</summary>
    public Task<ApiResponseEnvelope<CreateRcaResultDto>> CreateRcaAsync(
        CreateRcaRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateRcaRequest, CreateRcaResultDto>("v1/features/safety-events/root-cause-analysis", request, cancellationToken);

    // ============================================================
    // ContinuousImprovement.RCA write/lifecycle surface (api/v1/continuous-improvement/rca) —
    // Create -> StartAnalysis -> SubmitForApproval -> Close -> Archive, plus a real List (with
    // SiteId/From/To filters) and by-id GET. This is a distinct, generic RCA aggregate from the
    // Incident/NearMiss-linked RCA above (Features.SafetyEvents.RootCauseAnalysis, which has no
    // list endpoint and no lifecycle transitions) — this is where the module's real Search/Edit/
    // Delete-equivalent lifecycle actions actually live.
    // ============================================================

    /// <summary>Retrieves ContinuousImprovement RCA records for the given tenant/site/date-range. Backed by GET <c>v1/continuous-improvement/rca</c>.</summary>
    public Task<ApiResponseEnvelope<RcaRecordsResponseDto>> GetRcaRecordsAsync(
        Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var uri = $"v1/continuous-improvement/rca?From={Uri.EscapeDataString(fromUtc.ToString("O"))}" +
                  $"&To={Uri.EscapeDataString(toUtc.ToString("O"))}" +
                  (siteId.HasValue ? $"&SiteId={siteId.Value:D}" : string.Empty);
        return GetAsync<RcaRecordsResponseDto>(uri, cancellationToken);
    }

    /// <summary>Retrieves a single ContinuousImprovement RCA record by identity, including its ETag. Backed by GET <c>v1/continuous-improvement/rca/{id}</c>.</summary>
    public Task<ApiResponseEnvelope<RcaRecordDetailDto>> GetRcaRecordByIdAsync(
        Guid rcaId, CancellationToken cancellationToken = default)
        => GetAsync<RcaRecordDetailDto>($"v1/continuous-improvement/rca/{rcaId}", cancellationToken);

    /// <summary>Submits a new generic ContinuousImprovement RCA record via the real CreateRCACommand.</summary>
    public Task<ApiResponseEnvelope<RcaRecordStatusResultDto>> CreateRcaRecordAsync(
        CreateRcaRecordRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateRcaRecordRequest, RcaRecordStatusResultDto>("v1/continuous-improvement/rca", request, cancellationToken);

    /// <summary>Begins analysis work on the RCA record (Initiated -&gt; InProgress) via StartAnalysisRCACommand.</summary>
    public Task<ApiResponseEnvelope<RcaRecordStatusResultDto>> StartRcaRecordAnalysisAsync(
        Guid rcaId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<RcaEtagRequest, RcaRecordStatusResultDto>(
            $"v1/continuous-improvement/rca/{rcaId}/start-analysis", new RcaEtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Submits the RCA record for approval (InProgress -&gt; PendingApproval) via SubmitForApprovalRCACommand.</summary>
    public Task<ApiResponseEnvelope<RcaRecordStatusResultDto>> SubmitRcaRecordForApprovalAsync(
        Guid rcaId, SubmitRcaRecordForApprovalRequest request, CancellationToken cancellationToken = default)
        => PostAsync<SubmitRcaRecordForApprovalRequest, RcaRecordStatusResultDto>(
            $"v1/continuous-improvement/rca/{rcaId}/submit-for-approval", request, cancellationToken);

    /// <summary>Approves and formally closes the RCA record (PendingApproval -&gt; Closed) via CloseRCACommand.</summary>
    public Task<ApiResponseEnvelope<RcaRecordStatusResultDto>> CloseRcaRecordAsync(
        Guid rcaId, CloseRcaRecordRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CloseRcaRecordRequest, RcaRecordStatusResultDto>(
            $"v1/continuous-improvement/rca/{rcaId}/close", request, cancellationToken);

    /// <summary>Archives a closed RCA record for compliance retention (Closed -&gt; Archived) via ArchiveRCACommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<RcaRecordStatusResultDto>> ArchiveRcaRecordAsync(
        Guid rcaId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<RcaEtagRequest, RcaRecordStatusResultDto>(
            $"v1/continuous-improvement/rca/{rcaId}/archive", new RcaEtagRequest { ETag = eTag }, cancellationToken);
}
