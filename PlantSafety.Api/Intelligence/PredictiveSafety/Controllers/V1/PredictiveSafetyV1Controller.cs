// ============================================================================
// File: PredictiveSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V1 MVC controller exposing GET endpoints for the PredictiveSafety read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/intelligence/predictivesafety/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================
using PlantSafety.Api.Intelligence.PredictiveSafety.Contracts.Requests;
using PlantSafety.Api.Intelligence.PredictiveSafety.Contracts.Responses;
using PlantSafety.Api.Intelligence.PredictiveSafety.Authorization;
using PlantSafety.Application.Intelligence.PredictiveSafety.Queries;

namespace PlantSafety.Api.Intelligence.PredictiveSafety.Controllers.V1;

/// <summary>V1 controller for PredictiveSafety read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/predictivesafety")]
[Authorize(Policy = PredictiveSafetyAuthorizationPolicies.Read)]
[Tags("PredictiveSafety")]
public sealed class PredictiveSafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="PredictiveSafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public PredictiveSafetyV1Controller(ISender sender) => _sender = sender;

    /// <summary>Returns PredictiveSafety data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the PredictiveSafety result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PredictiveSafetyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetPredictiveSafetyRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetPredictiveSafetyQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<PredictiveSafetyResponse>.Fail(result.Error.Message, correlationId));

        var response = new PredictiveSafetyResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<PredictiveSafetyResponse>.Ok(response, correlationId));
    }
}