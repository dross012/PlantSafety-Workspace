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
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Commands;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Queries;
using PlantSafety.Application.Scheduling.PMScheduling.Models;
using PlantSafety.Application.Scheduling.PMScheduling.Queries;
using PlantSafety.Application.Scheduling.ShiftFatigue.Commands;
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
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

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
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single PMScheduling read-model record by identity, including its current ETag.</summary>
    [HttpGet("pm-scheduling/{pmSchedulingId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PMSchedulingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPMSchedulingByIdAsync(
        Guid pmSchedulingId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPMSchedulingByIdQuery(env.TenantId, pmSchedulingId);

        var result = await _sender.Send(query, cancellationToken);

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
    // Phase 4 full build-out (2026-07-30): added the write path (Schedule/Conduct/Complete/Close/
    // Archive) and the by-id read, appended additively alongside the pre-existing date-range list
    // endpoint below to minimize merge friction with sibling Scheduling sub-aggregate agents
    // concurrently editing this same shared controller.

    /// <summary>Schedules a new emergency drill.</summary>
    [HttpPost("emergency-drill-planning")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleEmergencyDrillPlanningResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleEmergencyDrillPlanningResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleEmergencyDrillPlanningAsync(
        [FromBody] ScheduleEmergencyDrillPlanningRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ScheduleEmergencyDrillPlanningCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            DrillType: request.DrillType,
            ShiftCode: request.ShiftCode,
            ScheduledAtUtc: request.ScheduledAtUtc,
            ParticipationCount: request.ParticipationCount,
            ScheduledByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that a scheduled drill was conducted (Created -&gt; Active).</summary>
    [HttpPost("emergency-drill-planning/{emergencyDrillPlanningId:guid}/conduct")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConductEmergencyDrillPlanningAsync(
        Guid emergencyDrillPlanningId,
        [FromBody] ConductEmergencyDrillPlanningRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ConductEmergencyDrillPlanningCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyDrillPlanningId: emergencyDrillPlanningId,
            ConductedByUserId: env.ActorId,
            ParticipationCount: request.ParticipationCount)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the post-drill debrief/completion (Active -&gt; Completed).</summary>
    [HttpPost("emergency-drill-planning/{emergencyDrillPlanningId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteEmergencyDrillPlanningAsync(
        Guid emergencyDrillPlanningId,
        [FromBody] CompleteEmergencyDrillPlanningRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteEmergencyDrillPlanningCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyDrillPlanningId: emergencyDrillPlanningId,
            PerformedByUserId: env.ActorId,
            DebriefNotes: request?.DebriefNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a completed drill record (Completed -&gt; Closed).</summary>
    [HttpPost("emergency-drill-planning/{emergencyDrillPlanningId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseEmergencyDrillPlanningAsync(
        Guid emergencyDrillPlanningId,
        [FromBody] CloseEmergencyDrillPlanningRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseEmergencyDrillPlanningCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyDrillPlanningId: emergencyDrillPlanningId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed drill record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("emergency-drill-planning/{emergencyDrillPlanningId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyDrillPlanningStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveEmergencyDrillPlanningAsync(
        Guid emergencyDrillPlanningId,
        [FromBody] ArchiveEmergencyDrillPlanningRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveEmergencyDrillPlanningCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EmergencyDrillPlanningId: emergencyDrillPlanningId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single EmergencyDrillPlanning read-model record by identity, including its current ETag.</summary>
    [HttpGet("emergency-drill-planning/{emergencyDrillPlanningId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models.EmergencyDrillPlanningItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyDrillPlanningByIdAsync(
        Guid emergencyDrillPlanningId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEmergencyDrillPlanningByIdQuery(env.TenantId, emergencyDrillPlanningId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

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
    // Phase 4 full build-out (2026-07-30): added the write path (Schedule/Activate/FlagConflict/
    // Complete/Close/Archive) and the by-id read, appended additively alongside the pre-existing
    // date-range list endpoint below to minimize merge friction with sibling Scheduling
    // sub-aggregate agents concurrently editing this same shared controller.

    /// <summary>Schedules a new HazardIsolationSequencing record.</summary>
    [HttpPost("hazard-isolation-sequencing")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleHazardIsolationSequencingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleHazardIsolationSequencingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleHazardIsolationSequencingAsync(
        [FromBody] ScheduleHazardIsolationSequencingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ScheduleHazardIsolationSequencingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            HazardZone: request.HazardZone,
            TaskType: request.TaskType,
            WindowStartUtc: request.WindowStartUtc,
            WindowEndUtc: request.WindowEndUtc,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates an isolation window (Created -&gt; Active).</summary>
    [HttpPost("hazard-isolation-sequencing/{hazardIsolationSequencingId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateHazardIsolationSequencingAsync(
        Guid hazardIsolationSequencingId,
        [FromBody] ActivateHazardIsolationSequencingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateHazardIsolationSequencingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazardIsolationSequencingId: hazardIsolationSequencingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags an incompatible-task conflict within an active isolation window.</summary>
    [HttpPost("hazard-isolation-sequencing/{hazardIsolationSequencingId:guid}/flag-conflict")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FlagHazardIsolationSequencingConflictAsync(
        Guid hazardIsolationSequencingId,
        [FromBody] FlagHazardIsolationSequencingConflictRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FlagHazardIsolationSequencingConflictCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazardIsolationSequencingId: hazardIsolationSequencingId,
            PerformedByUserId: env.ActorId,
            Reason: request.Reason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an isolation window (Active -&gt; Completed).</summary>
    [HttpPost("hazard-isolation-sequencing/{hazardIsolationSequencingId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteHazardIsolationSequencingAsync(
        Guid hazardIsolationSequencingId,
        [FromBody] CompleteHazardIsolationSequencingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteHazardIsolationSequencingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazardIsolationSequencingId: hazardIsolationSequencingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed isolation record (Completed -&gt; Closed).</summary>
    [HttpPost("hazard-isolation-sequencing/{hazardIsolationSequencingId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseHazardIsolationSequencingAsync(
        Guid hazardIsolationSequencingId,
        [FromBody] CloseHazardIsolationSequencingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseHazardIsolationSequencingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazardIsolationSequencingId: hazardIsolationSequencingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed isolation record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("hazard-isolation-sequencing/{hazardIsolationSequencingId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardIsolationSequencingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveHazardIsolationSequencingAsync(
        Guid hazardIsolationSequencingId,
        [FromBody] ArchiveHazardIsolationSequencingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveHazardIsolationSequencingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazardIsolationSequencingId: hazardIsolationSequencingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single HazardIsolationSequencing read-model record by identity, including its current ETag.</summary>
    [HttpGet("hazard-isolation-sequencing/{hazardIsolationSequencingId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models.HazardIsolationSequencingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardIsolationSequencingByIdAsync(
        Guid hazardIsolationSequencingId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazardIsolationSequencingByIdQuery(env.TenantId, hazardIsolationSequencingId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

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
    // Phase 4 full build-out (2026-07-30): added the write path (Record/Activate/ReassessRisk/
    // Complete/Close/Archive) and the by-id read, appended additively alongside the pre-existing
    // date-range list endpoint below to minimize merge friction with sibling Scheduling
    // sub-aggregate agents concurrently editing this same shared controller.

    /// <summary>Records a new ShiftFatigue assessment.</summary>
    [HttpPost("shift-fatigue")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordShiftFatigueResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordShiftFatigueResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordShiftFatigueAsync(
        [FromBody] RecordShiftFatigueRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordShiftFatigueCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            EmployeeId: request.EmployeeId,
            ShiftPattern: request.ShiftPattern,
            ConsecutiveNightShifts: request.ConsecutiveNightShifts,
            OvertimeHours: request.OvertimeHours,
            FatigueRiskScore: request.FatigueRiskScore,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates fatigue monitoring for an employee's shift record (Created -&gt; Active).</summary>
    [HttpPost("shift-fatigue/{shiftFatigueId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateShiftFatigueAsync(
        Guid shiftFatigueId,
        [FromBody] ActivateShiftFatigueRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateShiftFatigueCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ShiftFatigueId: shiftFatigueId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reassesses the fatigue risk score during active monitoring.</summary>
    [HttpPost("shift-fatigue/{shiftFatigueId:guid}/reassess-risk")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReassessShiftFatigueRiskAsync(
        Guid shiftFatigueId,
        [FromBody] ReassessShiftFatigueRiskRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReassessShiftFatigueRiskCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ShiftFatigueId: shiftFatigueId,
            PerformedByUserId: env.ActorId,
            FatigueRiskScore: request.FatigueRiskScore,
            ConsecutiveNightShifts: request.ConsecutiveNightShifts,
            OvertimeHours: request.OvertimeHours)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes the fatigue monitoring period (Active -&gt; Completed).</summary>
    [HttpPost("shift-fatigue/{shiftFatigueId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteShiftFatigueAsync(
        Guid shiftFatigueId,
        [FromBody] CompleteShiftFatigueRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteShiftFatigueCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ShiftFatigueId: shiftFatigueId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed fatigue record (Completed -&gt; Closed).</summary>
    [HttpPost("shift-fatigue/{shiftFatigueId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseShiftFatigueAsync(
        Guid shiftFatigueId,
        [FromBody] CloseShiftFatigueRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseShiftFatigueCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ShiftFatigueId: shiftFatigueId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed fatigue record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("shift-fatigue/{shiftFatigueId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftFatigueStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveShiftFatigueAsync(
        Guid shiftFatigueId,
        [FromBody] ArchiveShiftFatigueRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveShiftFatigueCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ShiftFatigueId: shiftFatigueId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single ShiftFatigue read-model record by identity, including its current ETag.</summary>
    [HttpGet("shift-fatigue/{shiftFatigueId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Scheduling.ShiftFatigue.Models.ShiftFatigueItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetShiftFatigueByIdAsync(
        Guid shiftFatigueId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetShiftFatigueByIdQuery(env.TenantId, shiftFatigueId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

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
