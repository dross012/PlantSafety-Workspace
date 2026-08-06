// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Training module. Wraps all HTTP
//   communication against the Operational Training read-model
//   (api/v1/operational/training). All methods return
//   ApiResponseEnvelope<T>. No HttpClient calls are permitted outside this
//   class for the Training module.
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
using PlantSafety.UI.Modules.Training.Models;

namespace PlantSafety.UI.Modules.Training.ApiClients;

/// <summary>
/// Typed API client for the Training module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class TrainingApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="TrainingApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public TrainingApiClient(HttpClient http, ILogger<TrainingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves training completion/assignment records for the given tenant within the
    /// inclusive UTC date range [<paramref name="from"/>, <paramref name="to"/>].
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the read-model response or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<ReadModelEnvelopeDto<TrainingRecordItemDto>>> GetTrainingRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => GetAsync<ReadModelEnvelopeDto<TrainingRecordItemDto>>(
            $"v1/operational/training?{BuildReadModelQuery(tenantId, siteId, from, to)}",
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

    /// <summary>
    /// Assigns a safety training course to a worker. Backed by
    /// POST <c>v1/features/operational/training</c> (requires the SafetyTier Approve policy) —
    /// a real, controller-wired write surface distinct from the read-only Operational Training
    /// list-model this client's <see cref="GetTrainingRecordsAsync"/> reads from. There is no
    /// field-level update or cancel/delete command for a training assignment; Assign is the
    /// only reachable write operation for this module.
    /// </summary>
    public Task<ApiResponseEnvelope<AssignTrainingResponse>> AssignTrainingAsync(
        AssignTrainingRequest request, CancellationToken cancellationToken = default)
        => PostAsync<AssignTrainingRequest, AssignTrainingResponse>(
            "v1/features/operational/training", request, cancellationToken);
}
