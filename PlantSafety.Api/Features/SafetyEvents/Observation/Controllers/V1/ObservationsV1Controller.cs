// ============================================================================
// File: ObservationsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer Features/SafetyEvents
//              Observation Commands/Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/observations/...
// - Envelope fields (CommandId/CorrelationId/TraceId/etc.) are populated via
//   RequestEnvelopeFactory rather than hand-rolled from HttpContext.Items.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer.
//   No domain logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.Observation.Contracts.Requests;
using PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Update;
using PlantSafety.Application.Features.SafetyEvents.Models.Observation;
using PlantSafety.Application.Features.SafetyEvents.Queries.Observation.Get;

namespace PlantSafety.Api.Features.SafetyEvents.Observation.Controllers.V1;

/// <summary>V1 controller for Safety Observation Commands and Queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/observations")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyEvents.Observations")]
public sealed class ObservationsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ObservationsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds canonical Command/Query envelope values for the current request.</param>
    public ObservationsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Safety Observation.</summary>
    /// <param name="request">Observation payload bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new Observation's identifier.</returns>
    /// <response code="201">Observation created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateObservationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateObservationCommand
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
            ObservationType = request.ObservationType,
            ObservedAtUtc = request.ObservedAtUtc,
            ObservedByWorkerId = request.ObservedByWorkerId
        };

        var observationId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { observationId },
            ApiResponse<Guid>.Ok(observationId, env.CorrelationId.ToString()));
    }

    /// <summary>Updates mutable fields of an existing Safety Observation.</summary>
    /// <param name="observationId">Observation to update.</param>
    /// <param name="request">Updated fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the updated Observation's identifier.</returns>
    /// <response code="200">Observation updated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPut("{observationId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid observationId,
        [FromBody] UpdateObservationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new UpdateObservationCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            ETag = request.ETag,
            ObservationId = observationId,
            Title = request.Title,
            Description = request.Description,
            ObservationType = request.ObservationType
        };

        var updatedId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(updatedId, env.CorrelationId.ToString()));
    }

    /// <summary>Returns the Safety Observation projection for a given identifier.</summary>
    /// <param name="observationId">Observation to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="ObservationDto"/>.</returns>
    /// <response code="200">Observation found.</response>
    /// <response code="404">Observation not found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{observationId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ObservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ObservationDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid observationId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetObservationByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            ObservationId = observationId
        };

        var observation = await _sender.Send(query, cancellationToken);

        if (observation is null)
            return NotFound(ApiResponse<ObservationDto>.Fail("Observation not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<ObservationDto>.Ok(observation, env.CorrelationId.ToString()));
    }
}
