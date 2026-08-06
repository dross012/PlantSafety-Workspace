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
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Close;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Complete;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.MarkOverdue;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Start;

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

    /// <summary>Transitions a CorrectiveAction from Open to InProgress.</summary>
    /// <param name="caId">Identifier of the CorrectiveAction to start.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Corrective action started successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">CorrectiveAction not found.</response>
    /// <response code="409">CorrectiveAction is not in a state that can be started.</response>
    [HttpPost("{caId:guid}/start")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateCorrectiveActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartAsync(Guid caId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartCorrectiveActionCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            CaId = caId,
        };

        var correctiveActionId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<CreateCorrectiveActionResponse>.Ok(
            new CreateCorrectiveActionResponse(correctiveActionId), env.CorrelationId.ToString()));
    }

    /// <summary>Marks a CorrectiveAction's work Completed (moves it to AwaitingVerification).</summary>
    /// <param name="caId">Identifier of the CorrectiveAction to complete.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Corrective action completed successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">CorrectiveAction not found.</response>
    /// <response code="409">CorrectiveAction is not in a state that can be completed.</response>
    [HttpPost("{caId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateCorrectiveActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteAsync(Guid caId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteCorrectiveActionCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            CaId = caId,
        };

        var correctiveActionId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<CreateCorrectiveActionResponse>.Ok(
            new CreateCorrectiveActionResponse(correctiveActionId), env.CorrelationId.ToString()));
    }

    /// <summary>Verifies and formally closes a CorrectiveAction (terminal state).</summary>
    /// <param name="caId">Identifier of the CorrectiveAction to close.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Corrective action closed successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">CorrectiveAction not found.</response>
    /// <response code="409">CorrectiveAction is not in a state that can be closed.</response>
    [HttpPost("{caId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateCorrectiveActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CloseAsync(Guid caId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseCorrectiveActionCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            CaId = caId,
        };

        var correctiveActionId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<CreateCorrectiveActionResponse>.Ok(
            new CreateCorrectiveActionResponse(correctiveActionId), env.CorrelationId.ToString()));
    }

    /// <summary>Marks a CorrectiveAction Overdue when its due date has passed without completion (idempotent no-op otherwise).</summary>
    /// <param name="caId">Identifier of the CorrectiveAction to mark overdue.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Request processed (may be a no-op if not eligible).</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">CorrectiveAction not found.</response>
    [HttpPost("{caId:guid}/mark-overdue")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateCorrectiveActionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkOverdueAsync(Guid caId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new MarkCorrectiveActionOverdueCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            CaId = caId,
        };

        var correctiveActionId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<CreateCorrectiveActionResponse>.Ok(
            new CreateCorrectiveActionResponse(correctiveActionId), env.CorrelationId.ToString()));
    }
}
