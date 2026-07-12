// ============================================================================
// File: DrillsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Create endpoint for the Features/SafetyEvents
//              Drills and Exercises command.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/drills/...
// - CreateDrillCommand carries the full correlation-lineage envelope (§2.1 of the Enterprise
//   Architectural Rule Handbook). RequestEnvelopeFactory builds those fields from the current
//   request's IOperationContext/ICurrentUserService.
//
// Rationale:
// - Routes all writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Requests;
using PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Responses;
using PlantSafety.Application.Features.SafetyEvents.Commands.DrillsAndExercises.Create;

namespace PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Controllers.V1;

/// <summary>V1 controller for Drills and Exercises command endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/drills")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Drills")]
public sealed class DrillsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DrillsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request.</param>
    public DrillsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Schedules a Safety Drill or Exercise.</summary>
    /// <param name="request">Request body describing the drill to schedule.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new drill identifier.</returns>
    /// <response code="201">Drill scheduled successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDrillResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDrillRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateDrillCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            Title = request.Title,
            Description = request.Description,
            DrillType = request.DrillType,
            ScheduledAtUtc = request.ScheduledAtUtc,
            CoordinatorId = request.CoordinatorId,
        };

        var drillId = await _sender.Send(command, cancellationToken);

        var response = new CreateDrillResponse(drillId);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CreateDrillResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
