// ============================================================================
// File: StopWorkEventsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the SafetyCulture module's StopWorkEvent commands over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/safetyculture/stop-work-events/...
// - This is a NEW SafetyCulture module, distinct from the pre-existing PlantSafety.Api.Analytics.Culture
//   tree; do not merge or relocate.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyCulture.Contracts.Requests;
using PlantSafety.Application.SafetyCulture.Commands;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.SafetyCulture.Controllers.V1;

/// <summary>V1 controller for Stop Work Event commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safetyculture/stop-work-events")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyCulture")]
public sealed class StopWorkEventsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="StopWorkEventsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public StopWorkEventsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new StopWorkEvent.</summary>
    /// <response code="200">Stop work event created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateStopWorkEventResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateStopWorkEventResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateStopWorkEventResult>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateStopWorkEventRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateStopWorkEventCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            OwnerId: env.ActorId,
            WorkArea: request.WorkArea,
            HazardDescription: request.HazardDescription);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records investigation findings for a StopWorkEvent.</summary>
    /// <response code="200">Investigation recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Stop work event not found.</response>
    [HttpPost("{stopWorkEventId:guid}/investigate")]
    [ProducesResponseType(typeof(ApiResponse<InvestigateStopWorkEventResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvestigateStopWorkEventResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InvestigateStopWorkEventResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InvestigateAsync(
        Guid stopWorkEventId,
        [FromBody] InvestigateStopWorkEventRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new InvestigateStopWorkEventCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            StopWorkEventId: stopWorkEventId,
            Findings: request.Findings)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves a StopWorkEvent.</summary>
    /// <response code="200">Stop work event resolved successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Stop work event not found.</response>
    [HttpPost("{stopWorkEventId:guid}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<ResolveStopWorkEventResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ResolveStopWorkEventResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ResolveStopWorkEventResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveAsync(
        Guid stopWorkEventId,
        [FromBody] ResolveStopWorkEventRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolveStopWorkEventCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            StopWorkEventId: stopWorkEventId,
            ResolutionSummary: request.ResolutionSummary,
            WasCelebrated: request.WasCelebrated)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
