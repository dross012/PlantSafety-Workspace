// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Culture dashboard module. Wraps
//   all HTTP communication against the 4 Analytics Culture read-models
//   (CultureIndex, Behavior, EngagementScoring, TrendModeling). All methods
//   return ApiResponseEnvelope<T>. No HttpClient calls are permitted outside
//   this class for the Culture module.
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
using PlantSafety.UI.Modules.Culture.Models;

namespace PlantSafety.UI.Modules.Culture.ApiClients;

/// <summary>
/// Typed API client for the Culture dashboard module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class CultureApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="CultureApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CultureApiClient(HttpClient http, ILogger<CultureApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves composite safety culture index snapshots for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<CultureIndexItemDto>>> GetCultureIndexAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<CultureIndexItemDto>>(
            $"v1/analytics/culture/cultureindex?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves behavior observations for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<BehaviorItemDto>>> GetBehaviorAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<BehaviorItemDto>>(
            $"v1/analytics/culture/behavior?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves employee engagement scores for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<EngagementScoringItemDto>>> GetEngagementScoringAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<EngagementScoringItemDto>>(
            $"v1/analytics/culture/engagementscoring?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves culture trend model outputs for the given tenant/date range.</summary>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<TrendModelingItemDto>>> GetTrendModelingAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<TrendModelingItemDto>>(
            $"v1/analytics/culture/trendmodeling?{BuildReadModelQuery(tenantId, siteId, from, to)}", cancellationToken);

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
