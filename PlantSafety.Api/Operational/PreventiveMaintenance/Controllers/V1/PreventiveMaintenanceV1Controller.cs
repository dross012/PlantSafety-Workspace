// ============================================================================
// File: PreventiveMaintenanceV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the PreventiveMaintenance read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/operational/preventive-maintenance/...
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
using PlantSafety.Api.Operational.PreventiveMaintenance.Contracts.Requests;
using PlantSafety.Api.Operational.PreventiveMaintenance.Contracts.Responses;
using PlantSafety.Api.Operational.PreventiveMaintenance.Authorization;
using PlantSafety.Application.Operational.PreventiveMaintenance.Commands;
using PlantSafety.Application.Operational.PreventiveMaintenance.Queries;

namespace PlantSafety.Api.Operational.PreventiveMaintenance.Controllers.V1;

/// <summary>
/// V1 controller for PreventiveMaintenance endpoints. Originally read-only (date-range list
/// query); the Phase 4 PreventiveMaintenanceTask full build-out (2026-07-30) adds the write
/// path (Schedule/Start/Complete/Verify/Cancel/MarkOverdue) and a by-id read query to this same
/// controller rather than introducing a sibling controller, mirroring the pre-existing
/// single-controller-per-leaf-module convention already used across PlantSafety.Api/Operational.
/// The pre-existing GET list action/route is left completely untouched.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational/preventive-maintenance")]
[Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Read)]
[Tags("PreventiveMaintenance")]
public sealed class PreventiveMaintenanceV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PreventiveMaintenanceV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PreventiveMaintenanceV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns PreventiveMaintenance data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the PreventiveMaintenance result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetPreventiveMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetPreventiveMaintenanceQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<PreventiveMaintenanceResponse>.Fail(result.Error.Message, correlationId));

        var response = new PreventiveMaintenanceResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<PreventiveMaintenanceResponse>.Ok(response, correlationId));
    }

    // ==== PreventiveMaintenanceTask write path (Phase 4 full build-out, 2026-07-30) ============

    /// <summary>Schedules a new PreventiveMaintenanceTask.</summary>
    [HttpPost]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SchedulePreventiveMaintenanceTaskResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SchedulePreventiveMaintenanceTaskResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleAsync(
        [FromBody] SchedulePreventiveMaintenanceTaskRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SchedulePreventiveMaintenanceTaskCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            EquipmentId: request.EquipmentId,
            TaskDescription: request.TaskDescription,
            ScheduledAtUtc: request.ScheduledAtUtc,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that maintenance work has begun (Scheduled -&gt; InProgress).</summary>
    [HttpPost("{preventiveMaintenanceTaskId:guid}/start")]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(
        Guid preventiveMaintenanceTaskId,
        [FromBody] StartPreventiveMaintenanceTaskRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartPreventiveMaintenanceTaskCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PreventiveMaintenanceTaskId: preventiveMaintenanceTaskId,
            StartedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that maintenance work has been completed (InProgress -&gt; Completed).</summary>
    [HttpPost("{preventiveMaintenanceTaskId:guid}/complete")]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid preventiveMaintenanceTaskId,
        [FromBody] CompletePreventiveMaintenanceTaskRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompletePreventiveMaintenanceTaskCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PreventiveMaintenanceTaskId: preventiveMaintenanceTaskId,
            CompletedByUserId: env.ActorId,
            CompletionNotes: request?.CompletionNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records independent verification of completed work (Completed -&gt; Verified).</summary>
    [HttpPost("{preventiveMaintenanceTaskId:guid}/verify")]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyAsync(
        Guid preventiveMaintenanceTaskId,
        [FromBody] VerifyPreventiveMaintenanceTaskRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new VerifyPreventiveMaintenanceTaskCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PreventiveMaintenanceTaskId: preventiveMaintenanceTaskId,
            VerifiedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Cancels a task before completion (Scheduled/InProgress -&gt; Cancelled).</summary>
    [HttpPost("{preventiveMaintenanceTaskId:guid}/cancel")]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelAsync(
        Guid preventiveMaintenanceTaskId,
        [FromBody] CancelPreventiveMaintenanceTaskRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CancelPreventiveMaintenanceTaskCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PreventiveMaintenanceTaskId: preventiveMaintenanceTaskId,
            CancelledByUserId: env.ActorId,
            Reason: request.Reason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a scheduled task overdue (Scheduled -&gt; Overdue).</summary>
    [HttpPost("{preventiveMaintenanceTaskId:guid}/mark-overdue")]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreventiveMaintenanceTaskStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkOverdueAsync(
        Guid preventiveMaintenanceTaskId,
        [FromBody] MarkPreventiveMaintenanceTaskOverdueRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new MarkPreventiveMaintenanceTaskOverdueCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PreventiveMaintenanceTaskId: preventiveMaintenanceTaskId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single PreventiveMaintenanceTask read-model record by identity, including its current ETag.</summary>
    [HttpGet("{preventiveMaintenanceTaskId:guid}")]
    [Authorize(Policy = PreventiveMaintenanceAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Operational.PreventiveMaintenance.Models.PreventiveMaintenanceTaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid preventiveMaintenanceTaskId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPreventiveMaintenanceTaskByIdQuery(env.TenantId, preventiveMaintenanceTaskId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}