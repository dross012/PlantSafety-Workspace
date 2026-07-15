// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Near Miss module. Wraps HTTP
//   communication for the Features/SafetyEvents/NearMisses CQRS endpoints
//   (list, get-by-id). No HttpClient calls are permitted outside this class
//   for the Near Miss module.
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
using PlantSafety.UI.Modules.NearMiss.Models;

namespace PlantSafety.UI.Modules.NearMiss.ApiClients;

/// <summary>Typed API client for the Near Miss module.</summary>
public sealed class NearMissApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="NearMissApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public NearMissApiClient(HttpClient http, ILogger<NearMissApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated, filterable list of near miss records for the authenticated tenant.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="potentialSeverity">Optional potential-severity filter.</param>
    /// <param name="hazardCategory">Optional hazard-category filter.</param>
    /// <param name="occurredFromUtc">Optional inclusive lower bound on occurrence date.</param>
    /// <param name="occurredToUtc">Optional inclusive upper bound on occurrence date.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<NearMissDto>>> ListNearMissesAsync(
        int pageNumber = 1,
        int pageSize = 25,
        Guid? siteId = null,
        string? potentialSeverity = null,
        string? hazardCategory = null,
        DateTimeOffset? occurredFromUtc = null,
        DateTimeOffset? occurredToUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = $"v1/features/safety-events/near-misses?pageNumber={pageNumber}&pageSize={pageSize}";
        if (siteId is not null) query += $"&siteId={siteId}";
        if (!string.IsNullOrWhiteSpace(potentialSeverity)) query += $"&potentialSeverity={Uri.EscapeDataString(potentialSeverity)}";
        if (!string.IsNullOrWhiteSpace(hazardCategory)) query += $"&hazardCategory={Uri.EscapeDataString(hazardCategory)}";
        if (occurredFromUtc is not null) query += $"&occurredFromUtc={occurredFromUtc:O}";
        if (occurredToUtc is not null) query += $"&occurredToUtc={occurredToUtc:O}";

        return GetAsync<PaginatedResponseDto<NearMissDto>>(query, cancellationToken);
    }

    /// <summary>Retrieves the full projection for a single near miss by its identifier.</summary>
    /// <param name="nearMissId">The unique near miss identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<NearMissDto>> GetNearMissAsync(
        Guid nearMissId,
        CancellationToken cancellationToken = default)
        => GetAsync<NearMissDto>($"v1/features/safety-events/near-misses/{nearMissId}", cancellationToken);
}
