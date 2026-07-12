// ============================================================================
// File: InternalV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V1 MVC controller exposing GET endpoints for the Internal read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/integrations/notifications/internal/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================
using PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Requests;
using PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Responses;
using PlantSafety.Api.Integrations.Notifications.Internal.Authorization;
using PlantSafety.Application.Integrations.Notifications.Internal.Queries;

namespace PlantSafety.Api.Integrations.Notifications.Internal.Controllers.V1;

/// <summary>V1 controller for Internal read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/integrations/notifications/internal")]
[Authorize(Policy = InternalAuthorizationPolicies.Read)]
[Tags("Internal")]
public sealed class InternalV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="InternalV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public InternalV1Controller(ISender sender) => _sender = sender;

    /// <summary>Returns Internal data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Internal result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<InternalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetInternalRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetInternalQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<InternalResponse>.Fail(result.Error.Message, correlationId));

        var response = new InternalResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<InternalResponse>.Ok(response, correlationId));
    }
}