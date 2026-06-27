// ============================================================================
// File: DOTV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for DOT. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the DOT contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Compliance.Certification.DOT.Contracts.Requests;
using PlantSafety.Api.Compliance.Certification.DOT.Contracts.Responses;
using PlantSafety.Api.Compliance.Certification.DOT.Authorization;

namespace PlantSafety.Api.Compliance.Certification.DOT.Controllers.V2;

/// <summary>V2 stub controller for DOT. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/compliance/certification/dot")]
[Authorize(Policy = DOTAuthorizationPolicies.Read)]
[Tags("DOT")]
public sealed class DOTV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="DOTV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public DOTV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for DOT. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with DOT data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DOTResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetDOTRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<DOTResponse>.Fail(
            "DOT V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}