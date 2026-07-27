// ============================================================================
// File: TrainingV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 controller exposing Operational/Training Commands from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/operational/training/...
// - Command envelope fields are populated via RequestEnvelopeFactory, which reads
//   IOperationContext and ICurrentUserService for the current request.
// - AssignTrainingCommandHandler returns a plain Guid, not Result<T>; unhandled
//   failures propagate to the global exception middleware.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Operational.Training.Contracts.Requests;
using PlantSafety.Api.Features.Operational.Training.Contracts.Responses;
using PlantSafety.Application.Features.Operational.Commands.Training.AssignTraining;

namespace PlantSafety.Api.Features.Operational.Training.Controllers.V1;

/// <summary>V1 controller for Operational/Training command endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/operational/training")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Training")]
public sealed class TrainingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="TrainingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public TrainingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Assigns a safety training course to a worker.</summary>
    /// <param name="request">Training assignment fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new training record identifier.</returns>
    /// <response code="200">Training assigned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AssignTrainingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignAsync(
        [FromBody] AssignTrainingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssignTrainingCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            WorkerId = request.WorkerId,
            TrainingCourseCode = request.TrainingCourseCode,
            TrainingCourseName = request.TrainingCourseName,
            DeliveryMethod = request.DeliveryMethod,
            DueDateUtc = request.DueDateUtc,
        };

        var trainingRecordId = await _sender.Send(command, cancellationToken);

        var response = new AssignTrainingResponse(trainingRecordId);

        return Ok(ApiResponse<AssignTrainingResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
