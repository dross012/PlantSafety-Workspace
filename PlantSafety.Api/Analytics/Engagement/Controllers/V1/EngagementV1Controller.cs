// ============================================================================
// File: EngagementV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 MVC controller exposing GET endpoints for the Engagement Analytics read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/analytics/engagement/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Analytics.Engagement.Contracts.Requests;
using PlantSafety.Api.Analytics.Engagement.Contracts.Responses;
using PlantSafety.Api.Analytics.Engagement.Authorization;
using PlantSafety.Application.Analytics.Queries;

namespace PlantSafety.Api.Analytics.Engagement.Controllers.V1;

/// <summary>V1 controller for Engagement analytics read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/engagement")]
[Authorize(Policy = EngagementAuthorizationPolicies.Read)]
[Tags("Analytics - Engagement")]
public sealed class EngagementV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="EngagementV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public EngagementV1Controller(ISender sender) => _sender = sender;

    /// <summary>Returns Engagement analytics data for the resolved tenant and optional site.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Engagement result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<EngagementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetEngagementRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetEngagementQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<EngagementResponse>.Fail(result.Error.Message, correlationId));

        var response = new EngagementResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Data.Select(d => new EngagementDataPoint(d.Label, d.Value, d.Trend)).ToList());

        return Ok(ApiResponse<EngagementResponse>.Ok(response, correlationId));
    }
}