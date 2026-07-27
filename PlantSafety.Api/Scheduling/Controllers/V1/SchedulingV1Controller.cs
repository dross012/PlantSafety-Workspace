// ============================================================================
// File: SchedulingV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Application-layer Scheduling module's Commands and
//              Queries over HTTP (PMScheduling, WorkOrderPriority, EmergencyDrillPlanning,
//              HazardIsolationSequencing, ShiftFatigue).
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/scheduling/...
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the
//   shared RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Scheduling.Contracts.Requests;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.Commands;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Queries;
using PlantSafety.Application.Scheduling.PMScheduling.Queries;
using PlantSafety.Application.Scheduling.ShiftFatigue.Queries;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Scheduling.Controllers.V1;

/// <summary>V1 controller for the Scheduling module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scheduling")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Scheduling")]
public sealed class SchedulingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SchedulingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public SchedulingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ── PMScheduling ─────────────────────────────────────────────────────────

    /// <summary>Places a preventive maintenance task on the PM calendar with a due date.</summary>
    [HttpPost("pm-scheduling/{pmSchedulingId:guid}/schedule")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleAsync(
        Guid pmSchedulingId,
        [FromBody] SchedulePreventiveMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SchedulePreventiveMaintenanceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PMSchedulingId: pmSchedulingId,
            DueAtUtc: request.DueAtUtc,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records completion of a preventive maintenance task.</summary>
    [HttpPost("pm-scheduling/{pmSchedulingId:guid}/record-completion")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordPMSchedulingCompletionAsync(
        Guid pmSchedulingId,
        [FromBody] RecordPMSchedulingCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordPMSchedulingCompletionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PMSchedulingId: pmSchedulingId,
            CompletionNotes: request.CompletionNotes,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Escalates an overdue preventive maintenance task with a Work Order Priority code (P1-P4).</summary>
    [HttpPost("pm-scheduling/{pmSchedulingId:guid}/escalate")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EscalatePMSchedulingAsync(
        Guid pmSchedulingId,
        [FromBody] EscalatePMSchedulingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EscalatePMSchedulingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PMSchedulingId: pmSchedulingId,
            PriorityCode: request.PriorityCode,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PMScheduling read-model data for a tenant/site within a date range.</summary>
    [HttpGet("pm-scheduling")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPMSchedulingAsync(
        [FromQuery] SchedulingListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPMSchedulingQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── WorkOrderPriority ────────────────────────────────────────────────────

    /// <summary>Escalates an overdue P1/P2 work order.</summary>
    [HttpPost("work-order-priority/{workOrderPriorityId:guid}/escalate")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderPriorityActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderPriorityActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EscalateWorkOrderPriorityAsync(
        Guid workOrderPriorityId,
        [FromBody] EscalateWorkOrderPriorityRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EscalateWorkOrderPriorityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkOrderPriorityId: workOrderPriorityId,
            EscalationReason: request.EscalationReason,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves the hazard addressed by a prioritized work order.</summary>
    [HttpPost("work-order-priority/{workOrderPriorityId:guid}/resolve")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderPriorityActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderPriorityActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveWorkOrderPriorityAsync(
        Guid workOrderPriorityId,
        [FromBody] ResolveWorkOrderPriorityRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolveWorkOrderPriorityCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkOrderPriorityId: workOrderPriorityId,
            ResolutionNotes: request.ResolutionNotes,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns WorkOrderPriority read-model data for a tenant/site within a date range.</summary>
    [HttpGet("work-order-priority")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderPriorityQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkOrderPriorityQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkOrderPriorityAsync(
        [FromQuery] SchedulingListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkOrderPriorityQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── EmergencyDrillPlanning ───────────────────────────────────────────────

    /// <summary>Returns EmergencyDrillPlanning read-model data for a tenant/site within a date range.</summary>
    [HttpGet("emergency-drill-planning")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyDrillPlanningAsync(
        [FromQuery] SchedulingListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEmergencyDrillPlanningQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── HazardIsolationSequencing ────────────────────────────────────────────

    /// <summary>Returns HazardIsolationSequencing read-model data for a tenant/site within a date range.</summary>
    [HttpGet("hazard-isolation-sequencing")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardIsolationSequencingAsync(
        [FromQuery] SchedulingListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazardIsolationSequencingQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── ShiftFatigue ─────────────────────────────────────────────────────────

    /// <summary>Returns ShiftFatigue read-model data for a tenant/site within a date range.</summary>
    [HttpGet("shift-fatigue")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetShiftFatigueAsync(
        [FromQuery] SchedulingListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetShiftFatigueQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Bulk ─────────────────────────────────────────────────────────────────

    /// <summary>Applies a single operation uniformly across multiple Scheduling resources.</summary>
    [HttpPost("bulk")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateSchedulingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateSchedulingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateAsync(
        [FromBody] BulkUpdateSchedulingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new BulkUpdateSchedulingCommand(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ResourceIds: request.ResourceIds,
            Operation: request.Operation,
            Parameters: request.Parameters)
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            ActorId = env.ActorId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

}
