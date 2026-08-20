// ============================================================================
// File: EnvironmentalMonitoringV2Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: V2 stub controller for EnvironmentalMonitoring. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the EnvironmentalMonitoring contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Operational.EnvironmentalMonitoring.Contracts.Requests;
using PlantSafety.Api.Operational.EnvironmentalMonitoring.Contracts.Responses;
using PlantSafety.Api.Operational.EnvironmentalMonitoring.Authorization;

namespace PlantSafety.Api.Operational.EnvironmentalMonitoring.Controllers.V2;

/// <summary>V2 stub controller for EnvironmentalMonitoring. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/operational/environmental-monitoring")]
[Authorize(Policy = EnvironmentalMonitoringAuthorizationPolicies.Read)]
[Tags("EnvironmentalMonitoring")]
public sealed class EnvironmentalMonitoringV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="EnvironmentalMonitoringV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public EnvironmentalMonitoringV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for EnvironmentalMonitoring. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with EnvironmentalMonitoring data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalMonitoringResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetEnvironmentalMonitoringRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<EnvironmentalMonitoringResponse>.Fail(
            "EnvironmentalMonitoring V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}