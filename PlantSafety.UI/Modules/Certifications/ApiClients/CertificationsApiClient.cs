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
}
