// ============================================================================
// File: InternalV2Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V2 stub controller for Internal. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the Internal contract needs a breaking change.
//
// ============================================================================
using PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Requests;
using PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Responses;
using PlantSafety.Api.Integrations.Notifications.Internal.Authorization;

namespace PlantSafety.Api.Integrations.Notifications.Internal.Controllers.V2;

/// <summary>V2 stub controller for Internal. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/integrations/notifications/internal")]
[Authorize(Policy = InternalAuthorizationPolicies.Read)]
[Tags("Internal")]
public sealed class InternalV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="InternalV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public InternalV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for Internal. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with Internal data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<InternalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetInternalRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<InternalResponse>.Fail(
            "Internal V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}