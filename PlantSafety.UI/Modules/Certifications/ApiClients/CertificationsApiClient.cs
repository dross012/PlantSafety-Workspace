// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Certifications module. Wraps all
//   HTTP communication against the 7 Compliance Certification read-models
//   (Contractor, DOT, EPA, IndustrySpecific, ISO14001, ISO45001,
//   OSHACertifications). All methods return ApiResponseEnvelope<T>. No
//   HttpClient calls are permitted outside this class for the Certifications
//   module.
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
using PlantSafety.UI.Modules.Certifications.Models;

namespace PlantSafety.UI.Modules.Certifications.ApiClients;

/// <summary>
/// Typed API client for the Certifications module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class CertificationsApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="CertificationsApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CertificationsApiClient(HttpClient http, ILogger<CertificationsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves contractor certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<ContractorCertItemDto>>> GetContractorCertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<ContractorCertItemDto>>(
            $"v1/compliance/certification/contractor?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves DOT certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<DotCertItemDto>>> GetDotCertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<DotCertItemDto>>(
            $"v1/compliance/certification/dot?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves EPA certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<EpaCertItemDto>>> GetEpaCertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<EpaCertItemDto>>(
            $"v1/compliance/certification/epa?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves industry-specific certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<IndustrySpecificCertItemDto>>> GetIndustrySpecificCertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<IndustrySpecificCertItemDto>>(
            $"v1/compliance/certification/industryspecific?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves ISO 14001 certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<Iso14001CertItemDto>>> GetIso14001CertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<Iso14001CertItemDto>>(
            $"v1/compliance/certification/iso14001?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves ISO 45001 certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<Iso45001CertItemDto>>> GetIso45001CertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<Iso45001CertItemDto>>(
            $"v1/compliance/certification/iso45001?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves OSHA certification records for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<OshaCertItemDto>>> GetOshaCertsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<OshaCertItemDto>>(
            $"v1/compliance/certification/oshacertifications?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

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
    // Compliance.Certification write/lifecycle surface (api/v1/compliance/certification) —
    // Create -> Verify -> Renew/Expire -> Close -> Archive, plus a real by-id GET. This is a
    // distinct, generic personnel-certification aggregate from the 7 read-only category
    // verticals above (Contractor/DOT/EPA/IndustrySpecific/ISO14001/ISO45001/OSHA) — those
    // have no write commands; this is where CreateCertificationCommand and its lifecycle
    // actions actually live.
    // ============================================================

    /// <summary>Retrieves a single Certification read-model record by identity, including its ETag.</summary>
    public Task<ApiResponseEnvelope<CertificationItemDto>> GetCertificationByIdAsync(
        Guid certificationId, CancellationToken cancellationToken = default)
        => GetAsync<CertificationItemDto>($"v1/compliance/certification/{certificationId}", cancellationToken);

    /// <summary>Submits a new Certification record via the real CreateCertificationCommand.</summary>
    public Task<ApiResponseEnvelope<CreateCertificationResultDto>> CreateCertificationAsync(
        CreateCertificationRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateCertificationRequest, CreateCertificationResultDto>("v1/compliance/certification", request, cancellationToken);

    /// <summary>Verifies and activates a Certification record (Created -&gt; Active) via VerifyCertificationCommand.</summary>
    public Task<ApiResponseEnvelope<CertificationStatusResultDto>> VerifyCertificationAsync(
        Guid certificationId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, CertificationStatusResultDto>(
            $"v1/compliance/certification/{certificationId}/verify", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Renews an active Certification record, extending its expiry, via RenewCertificationCommand.</summary>
    public Task<ApiResponseEnvelope<CertificationStatusResultDto>> RenewCertificationAsync(
        Guid certificationId, RenewCertificationRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RenewCertificationRequest, CertificationStatusResultDto>(
            $"v1/compliance/certification/{certificationId}/renew", request, cancellationToken);

    /// <summary>Expires an active Certification record past its validity window (Active -&gt; Completed) via ExpireCertificationCommand.</summary>
    public Task<ApiResponseEnvelope<CertificationStatusResultDto>> ExpireCertificationAsync(
        Guid certificationId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, CertificationStatusResultDto>(
            $"v1/compliance/certification/{certificationId}/expire", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Formally closes a completed/expired Certification record (Completed -&gt; Closed) via CloseCertificationCommand.</summary>
    public Task<ApiResponseEnvelope<CertificationStatusResultDto>> CloseCertificationAsync(
        Guid certificationId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, CertificationStatusResultDto>(
            $"v1/compliance/certification/{certificationId}/close", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Archives a closed Certification record for compliance retention (Closed -&gt; Archived) via ArchiveCertificationCommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<CertificationStatusResultDto>> ArchiveCertificationAsync(
        Guid certificationId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, CertificationStatusResultDto>(
            $"v1/compliance/certification/{certificationId}/archive", new EtagRequest { ETag = eTag }, cancellationToken);
}
