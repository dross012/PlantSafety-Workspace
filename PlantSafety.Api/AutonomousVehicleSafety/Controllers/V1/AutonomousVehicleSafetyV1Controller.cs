// ============================================================================
// File: AutonomousVehicleSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing every AutonomousVehicleSafety module command
//              (AutonomousVehicleAsset, OperatingGeofence, SafetyCaseValidation,
//              HumanRobotInteractionEvent, AutonomousVehicleIncident) and their read-model
//              "Get"/"List" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/autonomousvehiclesafety/{sub-component}/...
// - AutonomousVehicleSafety commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, ...domain
//   fields..., PerformedByUserId). PerformedByUserId is always populated from the current actor
//   (env.ActorId); aggregate identifiers are always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.AutonomousVehicleSafety.Contracts.Requests;
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Queries;

namespace PlantSafety.Api.AutonomousVehicleSafety.Controllers.V1;

/// <summary>V1 controller for the AutonomousVehicleSafety module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/autonomousvehiclesafety")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("AutonomousVehicleSafety")]
public sealed class AutonomousVehicleSafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AutonomousVehicleSafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AutonomousVehicleSafetyV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // AutonomousVehicleAsset (Autonomous Asset Registry)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new autonomous vehicle/mobile-robot asset.</summary>
    /// <response code="200">Registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("assets/register")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAutonomousVehicleAssetAsync([FromBody] RegisterAutonomousVehicleAssetRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterAutonomousVehicleAssetCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AssetTag, request.VehicleType, request.AutonomyLevel, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an autonomy-mode transition for an AutonomousVehicleAsset.</summary>
    /// <response code="200">Mode transition recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("assets/{autonomousVehicleAssetId:guid}/record-mode-transition")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAutonomyModeTransitionAsync(
        Guid autonomousVehicleAssetId, [FromBody] RecordAutonomyModeTransitionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAutonomyModeTransitionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            autonomousVehicleAssetId, request.ToMode, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends autonomous operation of an AutonomousVehicleAsset.</summary>
    /// <response code="200">Suspended.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("assets/{autonomousVehicleAssetId:guid}/suspend")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleAssetActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendAutonomousOperationAsync(
        Guid autonomousVehicleAssetId, [FromBody] SuspendAutonomousOperationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SuspendAutonomousOperationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            autonomousVehicleAssetId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single AutonomousVehicleAsset record by identifier.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("assets/{autonomousVehicleAssetId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models.AutonomousVehicleAssetItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAutonomousVehicleAssetByIdAsync(Guid autonomousVehicleAssetId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetAutonomousVehicleAssetByIdQuery(env.TenantId, autonomousVehicleAssetId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // OperatingGeofence (Geofence & Operating Domain Management)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Defines a new operating-domain geofence.</summary>
    /// <response code="200">Defined.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("geofences/define")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OperatingGeofenceActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OperatingGeofenceActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefineOperatingGeofenceAsync([FromBody] DefineOperatingGeofenceRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DefineOperatingGeofenceCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.GeofenceName, request.ZoneType, request.BoundaryDefinition, request.MaxSpeedKph, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns geofences with active violations for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("geofences/active-violations")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<OperatingGeofenceQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveGeofenceViolationsAsync([FromQuery] AutonomousVehicleSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListActiveGeofenceViolationsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SafetyCaseValidation (Safety Case Validation)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Submits and validates a safety case for an autonomous vehicle asset.</summary>
    /// <response code="200">Validated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("safety-cases/validate")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SafetyCaseValidationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyCaseValidationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidateSafetyCaseAsync([FromBody] ValidateSafetyCaseRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ValidateSafetyCaseCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AutonomousVehicleAssetId, request.StandardReference, request.DocumentVersion,
            request.RevalidationCadenceDays, request.ValidatedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the current safety-case validation status for an autonomous vehicle asset.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("safety-cases/status/{autonomousVehicleAssetId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<Application.AutonomousVehicleSafety.SafetyCaseValidation.Models.SafetyCaseValidationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSafetyCaseStatusAsync(Guid autonomousVehicleAssetId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSafetyCaseStatusQuery(env.TenantId, autonomousVehicleAssetId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HumanRobotInteractionEvent (Human-Robot Interaction Monitoring)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a new human-robot interaction event.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("human-robot-interactions/record")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HumanRobotInteractionEventActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HumanRobotInteractionEventActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordHumanRobotInteractionEventAsync([FromBody] RecordHumanRobotInteractionEventRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordHumanRobotInteractionEventCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AutonomousVehicleAssetId, request.InteractionType, request.SeverityLevel,
            request.DistanceMeters, request.LocationDescription, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HumanRobotInteractionEvent records for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("human-robot-interactions")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<HumanRobotInteractionEventQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListHumanRobotInteractionEventsAsync([FromQuery] AutonomousVehicleSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListHumanRobotInteractionEventsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // AutonomousVehicleIncident (Incident Management)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a new autonomous vehicle incident.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("incidents/record")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleIncidentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleIncidentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordAutonomousVehicleIncidentAsync([FromBody] RecordAutonomousVehicleIncidentRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAutonomousVehicleIncidentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AutonomousVehicleAssetId, request.IncidentType, request.SeverityLevel,
            request.DescriptionSummary, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the incident history for an autonomous vehicle asset.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("incidents/history/{autonomousVehicleAssetId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<AutonomousVehicleIncidentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIncidentHistoryByAssetAsync(Guid autonomousVehicleAssetId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetIncidentHistoryByAssetQuery(env.TenantId, autonomousVehicleAssetId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
