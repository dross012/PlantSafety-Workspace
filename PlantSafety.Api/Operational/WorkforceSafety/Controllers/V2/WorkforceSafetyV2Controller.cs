// ============================================================================
// File: WorkforceSafetyV2Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: V2 stub controller for WorkforceSafety. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the WorkforceSafety contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Operational.WorkforceSafety.Contracts.Requests;
using PlantSafety.Api.Operational.WorkforceSafety.Contracts.Responses;
using PlantSafety.Api.Operational.WorkforceSafety.Authorization;

namespace PlantSafety.Api.Operational.WorkforceSafety.Controllers.V2;

/// <summary>V2 stub controller for WorkforceSafety. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/operational/workforce-safety")]
[Authorize(Policy = WorkforceSafetyAuthorizationPolicies.Read)]
[Tags("WorkforceSafety")]
public sealed class WorkforceSafetyV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="WorkforceSafetyV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public WorkforceSafetyV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for WorkforceSafety. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with WorkforceSafety data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<WorkforceSafetyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetWorkforceSafetyRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<WorkforceSafetyResponse>.Fail(
            "WorkforceSafety V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}