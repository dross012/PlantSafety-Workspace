// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-05 — Added CreateDrillAsync (POST, wired to the real
//   CreateDrillCommand) and extended ListDrillsAsync with the Status/
//   DrillType/SiteId/OrderBy/SortDirection filter parameters the real
//   ListDrillsQuery already supports server-side.
// FILE DESCRIPTION: Typed API client for the Drills & Exercises module. Wraps
//   HTTP communication for scheduling drills (POST) and for the List/GetById
//   read endpoints under /v1/features/safety-events/drills, backed by
//   PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Controllers.V1
//   .DrillsV1Controller.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Drills.Models;

namespace PlantSafety.UI.Modules.Drills.ApiClients;

/// <summary>Typed API client for the Drills &amp; Exercises module.</summary>
public sealed class DrillsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="DrillsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public DrillsApiClient(HttpClient http, ILogger<DrillsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated, optionally filtered list of drills. Backed by GET v1/features/safety-events/drills.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="status">Optional status filter (e.g., Scheduled, Completed, Cancelled).</param>
    /// <param name="drillType">Optional drill type filter.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="orderBy">Field to order by. Defaults to ScheduledAtUtc.</param>
    /// <param name="sortDirection">ASC or DESC. Defaults to DESC.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<DrillDto>>> ListDrillsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        string? status = null,
        string? drillType = null,
        Guid? siteId = null,
        string orderBy = "ScheduledAtUtc",
        string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
    {
        var query = $"pageNumber={pageNumber}&pageSize={pageSize}" +
                    $"&orderBy={Uri.EscapeDataString(orderBy)}&sortDirection={Uri.EscapeDataString(sortDirection)}";

        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(drillType)) query += $"&drillType={Uri.EscapeDataString(drillType)}";
        if (siteId.HasValue) query += $"&siteId={siteId.Value}";

        return GetAsync<PaginatedResponseDto<DrillDto>>(
            $"v1/features/safety-events/drills?{query}",
            cancellationToken);
    }

    /// <summary>Retrieves a single drill by its identifier.</summary>
    /// <param name="drillId">The unique drill identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<DrillDto>> GetDrillAsync(
        Guid drillId,
        CancellationToken cancellationToken = default)
        => GetAsync<DrillDto>($"v1/features/safety-events/drills/{drillId}", cancellationToken);

    /// <summary>Schedules a new Safety Drill or Exercise. Backed by POST v1/features/safety-events/drills.</summary>
    /// <param name="request">The drill scheduling request.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<CreateDrillResponseDto>> CreateDrillAsync(
        CreateDrillRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateDrillRequest, CreateDrillResponseDto>(
            "v1/features/safety-events/drills", request, cancellationToken);
}
