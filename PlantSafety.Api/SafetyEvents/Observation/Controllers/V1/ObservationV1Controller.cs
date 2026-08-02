// ============================================================================
// File: ObservationV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the Observation read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/safety-events/observation/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.SafetyEvents.Observation.Contracts.Requests;
using PlantSafety.Api.SafetyEvents.Observation.Contracts.Responses;
using PlantSafety.Api.SafetyEvents.Observation.Authorization;
using PlantSafety.Application.SafetyEvents.Observation.Commands;
using PlantSafety.Application.SafetyEvents.Observation.Queries;

namespace PlantSafety.Api.SafetyEvents.Observation.Controllers.V1;

/// <summary>V1 controller for Observation read and write endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/observation")]
[Authorize(Policy = ObservationAuthorizationPolicies.Read)]
[Tags("Observation")]
public sealed class ObservationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ObservationV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ObservationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Records a new field observation (Phase 4 full build-out lifecycle write path).</summary>
    [HttpPost]
    [Authorize(Policy = ObservationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordObservationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordObservationResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordAsync(
        [FromBody] RecordObservationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordObservationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ObservationType: request.ObservationType,
            HazardCategory: request.HazardCategory,
            Location: request.Location,
            Description: request.Description,
            CorrectiveActionRequired: request.CorrectiveActionRequired,
            FollowUpDueAtUtc: request.FollowUpDueAtUtc,
            ObserverUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins follow-up tracking for an observation requiring corrective action (Created -&gt; Active).</summary>
    [HttpPost("{observationId:guid}/activate")]
    [Authorize(Policy = ObservationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid observationId,
        [FromBody] ActivateObservationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateObservationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ObservationId: observationId,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records completion of the required follow-up/corrective action (Active -&gt; Completed).</summary>
    [HttpPost("{observationId:guid}/complete")]
    [Authorize(Policy = ObservationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid observationId,
        [FromBody] CompleteObservationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteObservationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ObservationId: observationId,
            CompletedByUserId: env.ActorId,
            ResolutionNotes: request?.ResolutionNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a completed observation record (Completed -&gt; Closed).</summary>
    [HttpPost("{observationId:guid}/close")]
    [Authorize(Policy = ObservationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid observationId,
        [FromBody] CloseObservationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseObservationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ObservationId: observationId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed observation record for retention (Closed -&gt; Archived).</summary>
    [HttpPost("{observationId:guid}/archive")]
    [Authorize(Policy = ObservationAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ObservationStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid observationId,
        [FromBody] ArchiveObservationRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveObservationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ObservationId: observationId,
            ArchivedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Observation read-model record by identity, including its current ETag.</summary>
    [HttpGet("{observationId:guid}")]
    [Authorize(Policy = ObservationAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.Observation.Models.ObservationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid observationId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetObservationByIdQuery(env.TenantId, observationId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Observation data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Observation result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ObservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetObservationRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetObservationQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<ObservationResponse>.Fail(result.Error.Message, correlationId));

        var response = new ObservationResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<ObservationResponse>.Ok(response, correlationId));
    }
}