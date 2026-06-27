// ============================================================================
// File: PermissionsV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for Permissions. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the Permissions contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.CrossDomain.Permissions.Contracts.Requests;
using PlantSafety.Api.CrossDomain.Permissions.Contracts.Responses;
using PlantSafety.Api.CrossDomain.Permissions.Authorization;

namespace PlantSafety.Api.CrossDomain.Permissions.Controllers.V2;

/// <summary>V2 stub controller for Permissions. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/cross-domain/permissions")]
[Authorize(Policy = PermissionsAuthorizationPolicies.Read)]
[Tags("Permissions")]
public sealed class PermissionsV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="PermissionsV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public PermissionsV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for Permissions. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with Permissions data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PermissionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<PermissionsResponse>.Fail(
            "Permissions V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}