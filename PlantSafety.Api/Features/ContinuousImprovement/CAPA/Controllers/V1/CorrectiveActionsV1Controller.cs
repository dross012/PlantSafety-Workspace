// ============================================================================
// File: CorrectiveActionsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 controller exposing ContinuousImprovement/CAPA Commands from
//              the Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/continuous-improvement/capa/...
// - Command envelope fields are populated via RequestEnvelopeFactory, which reads
//   IOperationContext and ICurrentUserService for the current request.
// - CreateCorrectiveActionCommandHandler returns a plain Guid, not Result<T>;
//   unhandled failures propagate to the global exception middleware.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.ContinuousImprovement.CAPA.Contracts.Requests;
using PlantSafety.Api.Features.ContinuousImprovement.CAPA.Contracts.Responses;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;

namespace PlantSafety.Api.Features.ContinuousImprovement.CAPA.Controllers.V1;

/// <summary>V1 controller for ContinuousImprovement/CAPA command endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/continuous-improvement/capa")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CorrectiveActions")]
public sealed class CorrectiveActionsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CorrectiveActionsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public CorrectiveActionsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Corrective or Preventive Action.</summary>
    /// <param name="request">CAPA fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new CAPA identifier.</returns>
    /// <response code="200">Corrective action created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateCorrectiveActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCorrectiveActionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateCorrectiveActionCommand
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
            ActionType = request.ActionType,
            SourceEventId = request.SourceEventId,
            SourceEventType = request.SourceEventType,
            Priority = request.Priority,
            AssignedToWorkerId = request.AssignedToWorkerId,
            DueAtUtc = request.DueAtUtc,
        };

        var correctiveActionId = await _sender.Send(command, cancellationToken);

        var response = new CreateCorrectiveActionResponse(correctiveActionId);

        return Ok(ApiResponse<CreateCorrectiveActionResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
