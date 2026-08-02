// ============================================================================
// File: PredictiveMaintenanceAlertV1Controller.cs
// Project: PlantSafety.Api
// Module: Operational.PredictiveMaintenance
// Created: 2026-07-30
// Description: V1 controller exposing the Application-layer Operational.PredictiveMaintenance
//              module's write commands and by-id query over HTTP. Covers the full
//              PredictiveMaintenanceAlert lifecycle: Raise -> Review -> ScheduleAction ->
//              Resolve/Dismiss -> Close. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: routed under "api/v{version}/operational/predictive-maintenance/alerts" -- a
// distinct nested segment from the pre-existing read-only
// PredictiveMaintenanceV1Controller/PredictiveMaintenanceV2Controller
// ("api/v{version}/operational/predictive-maintenance", the date-range list endpoint backing
// GetPredictiveMaintenanceQuery, left completely untouched by this pass). The two controllers
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
using PlantSafety.Api.Operational.PredictiveMaintenance.Contracts.Requests;
using PlantSafety.Application.Operational.PredictiveMaintenance.Commands;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Operational.PredictiveMaintenance.Controllers.V1;

/// <summary>V1 controller for the Operational.PredictiveMaintenance module's alert commands and by-id query.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational/predictive-maintenance/alerts")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operational.PredictiveMaintenance")]
public sealed class PredictiveMaintenanceAlertV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PredictiveMaintenanceAlertV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PredictiveMaintenanceAlertV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Raises a new PredictiveMaintenanceAlert.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RaiseAsync(
        [FromBody] RaisePredictiveMaintenanceAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RaisePredictiveMaintenanceAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            EquipmentId: request.EquipmentId,
            PredictedFailureType: request.PredictedFailureType,
            PredictedAtUtc: request.PredictedAtUtc,
            RaisedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a reviewer's triage of the alert (Raised -&gt; UnderReview).</summary>
    [HttpPost("{predictiveMaintenanceAlertId:guid}/review")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReviewAsync(
        Guid predictiveMaintenanceAlertId,
        [FromBody] ReviewPredictiveMaintenanceAlertRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReviewPredictiveMaintenanceAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveMaintenanceAlertId: predictiveMaintenanceAlertId,
            ReviewedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules corrective maintenance action against the alert (UnderReview -&gt; ActionScheduled).</summary>
    [HttpPost("{predictiveMaintenanceAlertId:guid}/schedule-action")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleActionAsync(
        Guid predictiveMaintenanceAlertId,
        [FromBody] SchedulePredictiveMaintenanceAlertActionRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SchedulePredictiveMaintenanceAlertActionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveMaintenanceAlertId: predictiveMaintenanceAlertId,
            ScheduledByUserId: env.ActorId,
            WorkOrderReference: request?.WorkOrderReference)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that the predicted failure has been addressed (ActionScheduled -&gt; Resolved).</summary>
    [HttpPost("{predictiveMaintenanceAlertId:guid}/resolve")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveAsync(
        Guid predictiveMaintenanceAlertId,
        [FromBody] ResolvePredictiveMaintenanceAlertRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolvePredictiveMaintenanceAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveMaintenanceAlertId: predictiveMaintenanceAlertId,
            ResolvedByUserId: env.ActorId,
            ResolutionNotes: request?.ResolutionNotes)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Dismisses the alert without corrective action (Raised/UnderReview -&gt; Dismissed).</summary>
    [HttpPost("{predictiveMaintenanceAlertId:guid}/dismiss")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DismissAsync(
        Guid predictiveMaintenanceAlertId,
        [FromBody] DismissPredictiveMaintenanceAlertRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DismissPredictiveMaintenanceAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveMaintenanceAlertId: predictiveMaintenanceAlertId,
            DismissedByUserId: env.ActorId,
            Reason: request.Reason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a resolved or dismissed alert (Resolved/Dismissed -&gt; Closed).</summary>
    [HttpPost("{predictiveMaintenanceAlertId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceAlertStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid predictiveMaintenanceAlertId,
        [FromBody] ClosePredictiveMaintenanceAlertRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ClosePredictiveMaintenanceAlertCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PredictiveMaintenanceAlertId: predictiveMaintenanceAlertId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single PredictiveMaintenanceAlert read-model record by identity, including its current ETag.</summary>
    [HttpGet("{predictiveMaintenanceAlertId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Operational.PredictiveMaintenance.Models.PredictiveMaintenanceAlertItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid predictiveMaintenanceAlertId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPredictiveMaintenanceAlertByIdQuery(env.TenantId, predictiveMaintenanceAlertId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
