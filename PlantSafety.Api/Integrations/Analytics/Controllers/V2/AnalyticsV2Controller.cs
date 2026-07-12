// ============================================================================
// File: AnalyticsV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V2 stub controller for Integrations.Analytics. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the Integrations.Analytics contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Integrations.Analytics.Contracts.Requests;
using PlantSafety.Api.Integrations.Analytics.Contracts.Responses;
using PlantSafety.Api.Integrations.Analytics.Authorization;

namespace PlantSafety.Api.Integrations.Analytics.Controllers.V2;

/// <summary>V2 stub controller for Integrations.Analytics. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/integrations/analytics")]
[Authorize(Policy = AnalyticsAuthorizationPolicies.Read)]
[Tags("Analytics")]
public sealed class AnalyticsV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="AnalyticsV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public AnalyticsV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for Integrations.Analytics. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with Integrations.Analytics data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<AnalyticsResponse>.Fail(
            "Integrations.Analytics V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}
