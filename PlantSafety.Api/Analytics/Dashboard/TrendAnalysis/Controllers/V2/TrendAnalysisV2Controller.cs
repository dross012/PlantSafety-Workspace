// ============================================================================
// File: TrendAnalysisV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for TrendAnalysis. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the TrendAnalysis contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Contracts.Requests;
using PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Contracts.Responses;
using PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Authorization;

namespace PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Controllers.V2;

/// <summary>V2 stub controller for TrendAnalysis. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/analytics/dashboard/trendanalysis")]
[Authorize(Policy = TrendAnalysisAuthorizationPolicies.Read)]
[Tags("TrendAnalysis")]
public sealed class TrendAnalysisV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="TrendAnalysisV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public TrendAnalysisV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for TrendAnalysis. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with TrendAnalysis data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TrendAnalysisResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetTrendAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<TrendAnalysisResponse>.Fail(
            "TrendAnalysis V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}