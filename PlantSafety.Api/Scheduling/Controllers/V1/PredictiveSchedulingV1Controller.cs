// ============================================================================
// File: PredictiveSchedulingV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-03
// Description: V1 controller exposing the Scheduling.PredictiveScheduling module's OptimizationRun
//              request/read-model surface -- runs the real MultiObjectiveScheduleOptimizer over the
//              configured IOptimizationStrategy set and persists the resulting OptimizationRun
//              (POST request), and exposes by-id/history read-model queries (GET). Deliberately a
//              *separate* controller class sharing the "api/v{version}/scheduling" route prefix
//              with the pre-existing SchedulingV1Controller/SchedulingJobsV1Controller rather than
//              editing those large existing files, avoiding merge friction (multiple agents are
//              concurrently editing the Scheduling module). TenantId is always resolved
//              server-side via RequestEnvelopeFactory -- never bound from the client request body.
//              Routes all reads/writes through MediatR ISender to the Application layer. No domain
//              logic or data access here.
// ============================================================================
using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Scheduling.Authorization;
using PlantSafety.Api.Scheduling.Contracts.Requests;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Commands;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Queries;

namespace PlantSafety.Api.Scheduling.Controllers.V1;

/// <summary>V1 controller for the Scheduling.PredictiveScheduling module's OptimizationRun request/read-model surface.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scheduling/predictive")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Scheduling")]
public sealed class PredictiveSchedulingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PredictiveSchedulingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PredictiveSchedulingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>
    /// Runs the predictive scheduling optimization pipeline (MultiObjectiveScheduleOptimizer over
    /// the configured strategy set) against the supplied task/availability snapshot and persists
    /// the resulting OptimizationRun.
    /// </summary>
    /// <param name="request">Caller-supplied task/availability snapshot and objective.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("runs")]
    [Authorize(Policy = PredictiveSchedulingAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OptimizationRunDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OptimizationRunDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestRunAsync(
        [FromBody] RequestOptimizationRunRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RequestOptimizationRunCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Tasks: request.Tasks,
            Availability: request.Availability,
            ObjectiveCode: request.ObjectiveCode,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single OptimizationRun read-model record by identity.</summary>
    /// <param name="optimizationRunId">Identity of the OptimizationRun record to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("runs/{optimizationRunId:guid}")]
    [Authorize(Policy = PredictiveSchedulingAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<OptimizationRunDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRunAsync(
        Guid optimizationRunId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetOptimizationRunByIdQuery(env.TenantId, optimizationRunId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the OptimizationRun history for the caller's tenant, optionally scoped by site/date range/status.</summary>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="from">Inclusive start of the request date range (UTC). Defaults to 30 days before now.</param>
    /// <param name="to">Inclusive end of the request date range (UTC). Defaults to now.</param>
    /// <param name="status">Optional OptimizationRunStatus filter (e.g. "Completed").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("runs")]
    [Authorize(Policy = PredictiveSchedulingAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OptimizationRunSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] Guid? siteId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetOptimizationRunHistoryQuery(env.TenantId, siteId, from, to, status);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
