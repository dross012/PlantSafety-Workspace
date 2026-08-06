// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Auditing module. Wraps all HTTP
//   communication against the Compliance Auditing ComplianceAudit read-model
//   (api/v1/compliance/auditing/complianceaudit). All methods return
//   ApiResponseEnvelope<T>. No HttpClient calls are permitted outside this
//   class for the Auditing module.
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
using PlantSafety.UI.Modules.Auditing.Models;

namespace PlantSafety.UI.Modules.Auditing.ApiClients;

/// <summary>
/// Typed API client for the Auditing module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class AuditingApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="AuditingApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AuditingApiClient(HttpClient http, ILogger<AuditingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves compliance audit records for the given tenant within the inclusive UTC
    /// date range [<paramref name="from"/>, <paramref name="to"/>].
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the read-model response or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<AuditItemDto>>> GetAuditsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<AuditItemDto>>(
            $"v1/compliance/auditing/complianceaudit?{BuildReadModelQuery(tenantId, siteId, from, to)}",
            cancellationToken);

    /// <summary>
    /// Builds the canonical TenantId/SiteId/From/To query string shared by every
    /// PlantSafety.Api read-model endpoint (Compliance/Operational/CrossDomain/Analytics).
    /// </summary>
    private static string BuildReadModelQuery(Guid tenantId, Guid? siteId, DateTime from, DateTime to)
    {
        var query = $"TenantId={tenantId:D}&From={Uri.EscapeDataString(from.ToString("o"))}&To={Uri.EscapeDataString(to.ToString("o"))}";
        return siteId.HasValue ? $"{query}&SiteId={siteId.Value:D}" : query;
    }

    // ============================================================
    // Phase 5 §6.1 — AuditTimeline (GetAuditTimelineForEntityQuery)
    //
    // NOTE: as of this writing, PlantSafety.Api does not yet expose a V1 controller for the
    // Auditing.AuditTimeline sub-feature. This client targets the REST surface implied by
    // GetAuditTimelineForEntityQuery, mirroring the existing sub-feature controller convention
    // (GET {module}/{subfeature}?EntityType=&EntityId=). Calls degrade gracefully via
    // ErrorBoundaryComponent until that controller is added.
    // ============================================================

    /// <summary>Reconstructs the full chronological AuditTimeline for a single entity/aggregate.</summary>
    public Task<ApiResponseEnvelope<AuditTimelineDto>> GetAuditTimelineAsync(
        Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken = default)
        => GetAsync<AuditTimelineDto>(
            $"v1/auditing/audit-timeline?TenantId={tenantId:D}&EntityType={Uri.EscapeDataString(entityType)}&EntityId={entityId:D}",
            cancellationToken);

    // ============================================================
    // Phase 5 §6.2 — AuditReport (GenerateAuditReportCommand / GetAuditReportQuery /
    // GetAuditReportHistoryQuery / GetAuditReportContentQuery)
    //
    // NOTE: as of this writing, PlantSafety.Api does not yet expose a V1 controller for the
    // Auditing.AuditReport sub-feature. This client targets the REST surface implied by the
    // Application-layer command/query set. Calls degrade gracefully via ErrorBoundaryComponent
    // until that controller is added.
    // ============================================================

    /// <summary>Generates a printable/exportable audit report for a target entity.</summary>
    public Task<ApiResponseEnvelope<AuditReportDto>> GenerateAuditReportAsync(
        Guid tenantId, GenerateAuditReportRequest request, CancellationToken cancellationToken = default)
        => PostAsync<GenerateAuditReportRequest, AuditReportDto>(
            $"v1/auditing/audit-reports?TenantId={tenantId:D}", request, cancellationToken);

    /// <summary>Retrieves a single AuditReport by identity.</summary>
    public Task<ApiResponseEnvelope<AuditReportDto>> GetAuditReportAsync(
        Guid tenantId, Guid auditReportId, CancellationToken cancellationToken = default)
        => GetAsync<AuditReportDto>($"v1/auditing/audit-reports/{auditReportId}?TenantId={tenantId:D}", cancellationToken);

    /// <summary>Retrieves the AuditReport generation history for a target entity, most recent first.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<AuditReportDto>>> GetAuditReportHistoryAsync(
        Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<AuditReportDto>>(
            $"v1/auditing/audit-reports/history?TenantId={tenantId:D}&TargetEntityType={Uri.EscapeDataString(targetEntityType)}&TargetEntityId={targetEntityId:D}",
            cancellationToken);

    /// <summary>Retrieves the rendered byte-stream content of a generated AuditReport, for download.</summary>
    public Task<ApiResponseEnvelope<AuditReportContentDto>> GetAuditReportContentAsync(
        Guid tenantId, Guid auditReportId, CancellationToken cancellationToken = default)
        => GetAsync<AuditReportContentDto>(
            $"v1/auditing/audit-reports/{auditReportId}/content?TenantId={tenantId:D}", cancellationToken);

    // ============================================================
    // Compliance.Auditing write/lifecycle surface (api/v1/compliance/auditing) —
    // Create -> Start -> RecordFindings -> Close -> Archive, plus a real by-id GET.
    // ============================================================

    /// <summary>Retrieves a single Auditing read-model record by identity, including its ETag.</summary>
    public Task<ApiResponseEnvelope<AuditingItemDto>> GetAuditingByIdAsync(
        Guid auditingId, CancellationToken cancellationToken = default)
        => GetAsync<AuditingItemDto>($"v1/compliance/auditing/{auditingId}", cancellationToken);

    /// <summary>Submits a new Auditing record via the real CreateAuditingCommand.</summary>
    public Task<ApiResponseEnvelope<CreateAuditingResultDto>> CreateAuditingAsync(
        CreateAuditingRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateAuditingRequest, CreateAuditingResultDto>("v1/compliance/auditing", request, cancellationToken);

    /// <summary>Starts an audit program/execution (Created -&gt; Active) via StartAuditingCommand.</summary>
    public Task<ApiResponseEnvelope<AuditingStatusResultDto>> StartAuditingAsync(
        Guid auditingId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, AuditingStatusResultDto>(
            $"v1/compliance/auditing/{auditingId}/start", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Records an audit's findings and compliance score (Active -&gt; Completed) via RecordAuditingFindingsCommand.</summary>
    public Task<ApiResponseEnvelope<AuditingStatusResultDto>> RecordAuditingFindingsAsync(
        Guid auditingId, RecordAuditingFindingsRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RecordAuditingFindingsRequest, AuditingStatusResultDto>(
            $"v1/compliance/auditing/{auditingId}/record-findings", request, cancellationToken);

    /// <summary>Formally closes a completed audit (Completed -&gt; Closed) via CloseAuditingCommand.</summary>
    public Task<ApiResponseEnvelope<AuditingStatusResultDto>> CloseAuditingAsync(
        Guid auditingId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, AuditingStatusResultDto>(
            $"v1/compliance/auditing/{auditingId}/close", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Archives a closed audit for compliance retention (Closed -&gt; Archived) via ArchiveAuditingCommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<AuditingStatusResultDto>> ArchiveAuditingAsync(
        Guid auditingId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, AuditingStatusResultDto>(
            $"v1/compliance/auditing/{auditingId}/archive", new EtagRequest { ETag = eTag }, cancellationToken);
}
