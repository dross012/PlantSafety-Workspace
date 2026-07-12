// ============================================================================
// File: EmergencyResponsesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Create endpoint for the Features/SafetyEvents
//              Emergency Management command.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/emergency-responses/...
// - CreateEmergencyResponseCommand carries the full correlation-lineage envelope (§2.1 of the
//   Enterprise Architectural Rule Handbook). RequestEnvelopeFactory builds those fields from
//   the current request's IOperationContext/ICurrentUserService.
//
// Rationale:
// - Routes all writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.EmergencyManagement.Contracts.Requests;
using PlantSafety.Api.Features.SafetyEvents.EmergencyManagement.Contracts.Responses;
using PlantSafety.Application.Features.SafetyEvents.Commands.EmergencyManagement.Create;

namespace PlantSafety.Api.Features.SafetyEvents.EmergencyManagement.Controllers.V1;

/// <summary>V1 controller for Emergency Management command endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/emergency-responses")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EmergencyResponses")]
public sealed class EmergencyResponsesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EmergencyResponsesV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request.</param>
    public EmergencyResponsesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Activates an Emergency Response plan.</summary>
    /// <param name="request">Request body describing the emergency response to activate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new response identifier.</returns>
    /// <response code="201">Emergency response activated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateEmergencyResponseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateEmergencyResponseRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateEmergencyResponseCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            LinkedCrisisEventId = request.LinkedCrisisEventId,
            ResponsePlanName = request.ResponsePlanName,
            Description = request.Description,
            ActivatedAtUtc = request.ActivatedAtUtc,
            CoordinatorId = request.CoordinatorId,
        };

        var responseId = await _sender.Send(command, cancellationToken);

        var response = new CreateEmergencyResponseResponse(responseId);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CreateEmergencyResponseResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
