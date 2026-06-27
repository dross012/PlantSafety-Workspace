// ============================================================================
// File: PreventiveMaintenanceV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the PreventiveMaintenance read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/operational/preventive-maintenance/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Operational.PreventiveMaintenance.Contracts.Requests;
using PlantSafety.Api.Operational.PreventiveMaintenance.Contracts.Responses;
using PlantSafety.Api.Operational.PreventiveMaintenance.Authorization;
using PlantSafety.Application.Operational.PreventiveMaintenance.Queries;

namespace PlantSafety.Api.Operational.PreventiveMaintenance.Controllers.V1;

/// <summary>V1 controller for PreventiveMaintenance read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational/preventive-maintenance")]
[Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Read)]
[Tags("PreventiveMaintenance")]
public sealed class PreventiveMaintenanceV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="PreventiveMaintenanceV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public PreventiveMaintenanceV1Controller(ISender sender) => _sender = sender;

    /// <summary>Returns PreventiveMaintenance data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the PreventiveMaintenance result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetPreventiveMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetPreventiveMaintenanceQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<PreventiveMaintenanceResponse>.Fail(result.Error.Message, correlationId));

        var response = new PreventiveMaintenanceResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<PreventiveMaintenanceResponse>.Ok(response, correlationId));
    }
}