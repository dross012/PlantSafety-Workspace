// ============================================================================
// File: IncidentPredictionV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the IncidentPrediction read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/analytics/predictive/incidentprediction/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Contracts.Requests;
using PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Contracts.Responses;
using PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Authorization;
using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Queries;

namespace PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Controllers.V1;

/// <summary>V1 controller for IncidentPrediction read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/predictive/incidentprediction")]
[Authorize(Policy = IncidentPredictionAuthorizationPolicies.Read)]
[Tags("IncidentPrediction")]
public sealed class IncidentPredictionV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="IncidentPredictionV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public IncidentPredictionV1Controller(ISender sender) => _sender = sender;

    /// <summary>Returns IncidentPrediction data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the IncidentPrediction result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IncidentPredictionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetIncidentPredictionRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetIncidentPredictionQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IncidentPredictionResponse>.Fail(result.Error.Message, correlationId));

        var response = new IncidentPredictionResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<IncidentPredictionResponse>.Ok(response, correlationId));
    }
}