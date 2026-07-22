// ============================================================================
// File: ScorecardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing every Scorecards module command (open + 5
//              compilation/distribution lifecycle commands) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Envelope fields are populated via RequestEnvelopeFactory, mirroring the canonical
//   Features/Compliance/Permits pattern. Commands are dispatched through MediatR ISender.
// - ScorecardCompilationCommands.cs commands implement MediatR IRequest<Result<T>> directly
//   (not the ICommand<T> marker) and therefore carry no ApiVersionToken/ActorId fields; the
//   "*By" field on each command IS the acting user and is populated from the envelope ActorId.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.Scorecards.Contracts.Requests;
using PlantSafety.Application.Scorecards.Commands;

namespace PlantSafety.Api.Scorecards.Controllers.V1;

/// <summary>V1 controller for Scorecards open + compilation/distribution lifecycle commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scorecards")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Scorecards")]
public sealed class ScorecardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ScorecardsV1Controller"/>.</summary>
    public ScorecardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Opens a new Scorecard reporting period.</summary>
    /// <response code="200">Scorecard opened successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateScorecardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CreateScorecardResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateScorecardRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = CreateScorecardCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            scorecardTitle: request.ScorecardTitle,
            category: request.Category,
            periodType: request.PeriodType,
            periodStartUtc: request.PeriodStartUtc,
            ownerId: request.OwnerId,
            reviewRequired: request.ReviewRequired,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken) with { ActorId = env.ActorId };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records (or overwrites) a KPI measurement for the scorecard's reporting period.</summary>
    /// <response code="200">Metric recorded successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{scorecardId:guid}/metrics")]
    [ProducesResponseType(typeof(ApiResponse<RecordScorecardMetricResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RecordScorecardMetricResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordMetricAsync(Guid scorecardId, [FromBody] RecordScorecardMetricRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordScorecardMetricCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, scorecardId, request.Metric, request.Numerator, request.Denominator, request.DataSourceModule, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks KPI compilation complete for the reporting period.</summary>
    /// <response code="200">Compilation completed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{scorecardId:guid}/complete-compilation")]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteCompilationAsync(Guid scorecardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteScorecardCompilationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, scorecardId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a Compiled scorecard.</summary>
    /// <response code="200">Scorecard published successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{scorecardId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PublishAsync(Guid scorecardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishScorecardCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, scorecardId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a distribution event for a Published scorecard.</summary>
    /// <response code="200">Distribution recorded successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{scorecardId:guid}/distribute")]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordDistributionAsync(Guid scorecardId, [FromBody] RecordScorecardDistributionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordScorecardDistributionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, scorecardId, request.Channel, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes the scorecard once its reporting period has ended.</summary>
    /// <response code="200">Scorecard closed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{scorecardId:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScorecardStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid scorecardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseScorecardCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, scorecardId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
