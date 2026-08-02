// ============================================================================
// File: DrillsAndExercisesActionsV1Controller.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.DrillsAndExercises
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer SafetyEvents.DrillsAndExercises
//              module's write commands and by-id query over HTTP. Covers the full
//              DrillsAndExercises lifecycle: Schedule -> Conduct -> Complete / Cancel -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Operational.PredictiveMaintenance.Controllers.V1
//              .PredictiveMaintenanceAlertV1Controller.
//
// Route note: routed under "api/v{version}/safety-events/drills-and-exercises/actions" -- a
// distinct nested segment from the pre-existing read-only DrillsAndExercisesV1Controller
// ("api/v{version}/safety-events/drills-and-exercises", the date-range list endpoint backing
// GetDrillsAndExercisesQuery, left completely untouched by this pass). The two controllers
// therefore never collide on route + verb even though they share the same module segment prefix.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyEvents.DrillsAndExercises.Contracts.Requests;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Controllers.V1;

/// <summary>V1 controller for the SafetyEvents.DrillsAndExercises module's write commands and by-id query.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/drills-and-exercises/actions")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyEvents.DrillsAndExercises")]
public sealed class DrillsAndExercisesActionsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DrillsAndExercisesActionsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public DrillsAndExercisesActionsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Schedules a new DrillsAndExercises drill.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleAsync(
        [FromBody] ScheduleDrillsAndExercisesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ScheduleDrillsAndExercisesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            DrillType: request.DrillType,
            ScheduledAtUtc: request.ScheduledAtUtc,
            ScheduledByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that a scheduled drill was conducted (Scheduled -&gt; Conducted).</summary>
    [HttpPost("{drillsAndExercisesId:guid}/conduct")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConductAsync(
        Guid drillsAndExercisesId,
        [FromBody] ConductDrillsAndExercisesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ConductDrillsAndExercisesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DrillsAndExercisesId: drillsAndExercisesId,
            ConductedByUserId: env.ActorId,
            ParticipantCount: request.ParticipantCount)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a conducted drill, recording the debrief (Conducted -&gt; Completed).</summary>
    [HttpPost("{drillsAndExercisesId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid drillsAndExercisesId,
        [FromBody] CompleteDrillsAndExercisesRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteDrillsAndExercisesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DrillsAndExercisesId: drillsAndExercisesId,
            CompletedByUserId: env.ActorId,
            DebriefNotes: request?.DebriefNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Cancels a scheduled drill before it is conducted (Scheduled -&gt; Cancelled).</summary>
    [HttpPost("{drillsAndExercisesId:guid}/cancel")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelAsync(
        Guid drillsAndExercisesId,
        [FromBody] CancelDrillsAndExercisesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CancelDrillsAndExercisesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DrillsAndExercisesId: drillsAndExercisesId,
            CancelledByUserId: env.ActorId,
            Reason: request.Reason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed drill record (Completed -&gt; Closed).</summary>
    [HttpPost("{drillsAndExercisesId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid drillsAndExercisesId,
        [FromBody] DrillsAndExercisesLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseDrillsAndExercisesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DrillsAndExercisesId: drillsAndExercisesId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed drill record (Closed -&gt; Archived).</summary>
    [HttpPost("{drillsAndExercisesId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DrillsAndExercisesStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid drillsAndExercisesId,
        [FromBody] DrillsAndExercisesLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveDrillsAndExercisesCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DrillsAndExercisesId: drillsAndExercisesId,
            ArchivedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single DrillsAndExercises read-model record by identity, including its current ETag.</summary>
    [HttpGet("{drillsAndExercisesId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models.DrillsAndExercisesDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid drillsAndExercisesId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDrillsAndExercisesByIdQuery(env.TenantId, drillsAndExercisesId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
