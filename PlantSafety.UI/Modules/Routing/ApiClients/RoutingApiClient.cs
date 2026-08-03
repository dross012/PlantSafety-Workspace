// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-03 -- Wired to the real ProcessManagement.Routing V1 controller
//   (PlantSafety.Api.ProcessManagement.Routing.Controllers.V1.RouteDefinitionV1Controller,
//   route prefix api/v1/routing/routings) backed by a genuinely new RouteDefinition/
//   RouteVersionSnapshot domain model that partially reuses ProcessManagement's
//   approval/versioning lifecycle. See docs/Phase5/ui-gaps-scoping.md §9.
// FILE DESCRIPTION: Typed API client for the Routing module (Volume R1).
//   Wraps all HTTP communication for the Dashboard, Detail, Steps, Version
//   History, Diff Viewer, Simulation Workspace, and Approval Workflow
//   pages. All methods return ApiResponseEnvelope<T>. Pure transport
//   adapter — no diff/simulation/approval-stage logic in this class.
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
using PlantSafety.UI.Modules.Routing.Models;

namespace PlantSafety.UI.Modules.Routing.ApiClients;

/// <summary>Typed API client for the Routing module.</summary>
public sealed class RoutingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="RoutingApiClient"/>.</summary>
    public RoutingApiClient(HttpClient http, ILogger<RoutingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated page of routings for the dashboard list.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<RoutingSummaryDto>>> GetRoutingsAsync(
        int pageNumber = 1, int pageSize = 25, string? search = null, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<RoutingSummaryDto>>(
            $"v1/routing/routings?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(search) ? string.Empty : $"&search={Uri.EscapeDataString(search)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail/metadata for a single routing.</summary>
    public Task<ApiResponseEnvelope<RoutingDetailDto>> GetRoutingDetailAsync(Guid routingId, CancellationToken cancellationToken = default)
        => GetAsync<RoutingDetailDto>($"v1/routing/routings/{routingId}", cancellationToken);

    /// <summary>Retrieves the ordered step list for a routing.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<RoutingStepDto>>> GetStepsAsync(Guid routingId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<RoutingStepDto>>($"v1/routing/routings/{routingId}/steps", cancellationToken);

    /// <summary>Retrieves the version history timeline for a routing.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<RoutingVersionDto>>> GetVersionHistoryAsync(Guid routingId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<RoutingVersionDto>>($"v1/routing/routings/{routingId}/versions", cancellationToken);

    /// <summary>Retrieves the server-computed, pre-aligned diff between two versions.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<RoutingDiffRowDto>>> GetDiffAsync(
        Guid routingId, Guid previousVersionId, Guid currentVersionId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<RoutingDiffRowDto>>(
            $"v1/routing/routings/{routingId}/diff?previousVersionId={previousVersionId}&currentVersionId={currentVersionId}", cancellationToken);

    /// <summary>Runs a server-side simulation for a routing (no critical-path/bottleneck logic in UI).</summary>
    public Task<ApiResponseEnvelope<RoutingSimulationResultDto>> RunSimulationAsync(Guid routingId, CancellationToken cancellationToken = default)
        => GetAsync<RoutingSimulationResultDto>($"v1/routing/routings/{routingId}/simulation", cancellationToken);

    /// <summary>Retrieves the current approval workflow state for a routing.</summary>
    public Task<ApiResponseEnvelope<RoutingApprovalWorkflowDto>> GetApprovalWorkflowAsync(Guid routingId, CancellationToken cancellationToken = default)
        => GetAsync<RoutingApprovalWorkflowDto>($"v1/routing/routings/{routingId}/approval", cancellationToken);
}
