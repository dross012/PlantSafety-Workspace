// ============================================================================
// File: EvacuationModelingV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing every DigitalTwin.EvacuationModeling module command
//              (EvacuationRoutes, EvacuationZones, EgressSimulations, EvacuationBottlenecks) and
//              their read-model "Get"/"List" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/digitaltwin/evacuation-modeling/{sub-component}/...
// - Commands/queries return PlantSafety.Application.Common.Results.Result<T>, mapped to
//   ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, ...domain
//   fields..., PerformedByUserId). PerformedByUserId is always populated from the current actor
//   (env.ActorId); the aggregate id is always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.DigitalTwin.EvacuationModeling.Contracts.Requests;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Queries;

namespace PlantSafety.Api.DigitalTwin.EvacuationModeling.Controllers.V1;

/// <summary>V1 controller for the DigitalTwin.EvacuationModeling module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/digitaltwin/evacuation-modeling")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("DigitalTwin.EvacuationModeling")]
public sealed class EvacuationModelingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EvacuationModelingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public EvacuationModelingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // EvacuationRoutes
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Defines a new evacuation route against a validated Digital Twin model version.</summary>
    /// <response code="200">Route defined.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("routes")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefineEvacuationRouteAsync([FromBody] DefineEvacuationRouteRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DefineEvacuationRouteCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, request.SiteId,
            request.RouteName, request.ZoneId, request.AssemblyPointId, request.PathGeometryRef,
            request.TwinModelVersion, request.EstimatedEgressTimeSeconds, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires an evacuation route.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("routes/{evacuationRouteId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireEvacuationRouteAsync(Guid evacuationRouteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireEvacuationRouteCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, evacuationRouteId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an obstruction detected on an evacuation route (e.g. via BMS feed integration).</summary>
    /// <response code="200">Obstruction recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("routes/{evacuationRouteId:guid}/record-obstruction")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordRouteObstructionAsync(
        Guid evacuationRouteId, [FromBody] RecordRouteObstructionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordRouteObstructionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            evacuationRouteId, request.Description, request.Severity, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Clears an open obstruction on an evacuation route.</summary>
    /// <response code="200">Obstruction cleared.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("routes/{evacuationRouteId:guid}/clear-obstruction")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationRouteActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearRouteObstructionAsync(
        Guid evacuationRouteId, [FromBody] ClearRouteObstructionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ClearRouteObstructionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            evacuationRouteId, request.RouteObstructionId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single evacuation route by identity.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("routes/{evacuationRouteId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEvacuationRouteByIdAsync(Guid evacuationRouteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEvacuationRouteByIdQuery(env.TenantId, evacuationRouteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists evacuation routes belonging to a given evacuation zone.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("zones/{zoneId:guid}/routes")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListEvacuationRoutesByZoneAsync(Guid zoneId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListEvacuationRoutesByZoneQuery(env.TenantId, zoneId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists all currently open (uncleared) route obstructions for the tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("routes/active-obstructions")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveObstructionsAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListActiveObstructionsQuery(env.TenantId, siteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EvacuationZones
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Defines a new evacuation zone against a validated Digital Twin model version.</summary>
    /// <response code="200">Zone defined.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("zones")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationZoneActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationZoneActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefineEvacuationZoneAsync([FromBody] DefineEvacuationZoneRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DefineEvacuationZoneCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, request.SiteId,
            request.ZoneName, request.TwinModelVersion, request.ZoneCapacity, request.OccupancyThreshold, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an occupancy snapshot for an evacuation zone (e.g. via BMS feed integration).</summary>
    /// <response code="200">Occupancy snapshot recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("zones/{evacuationZoneId:guid}/record-occupancy-snapshot")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationZoneActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationZoneActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EvacuationZoneActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordOccupancySnapshotAsync(
        Guid evacuationZoneId, [FromBody] RecordOccupancySnapshotRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordOccupancySnapshotCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            evacuationZoneId, request.HeadcountEstimate, request.Source, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves occupancy readings for an evacuation zone within a date range (feeds the occupancy-reading-freshness KPI).</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("zones/{zoneId:guid}/occupancy")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOccupancyByZoneAsync(
        Guid zoneId, [FromQuery] EvacuationModelingQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetOccupancyByZoneQuery(env.TenantId, request.SiteId, zoneId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EgressSimulations
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Runs a new egress simulation for a zone, optionally correlated with a specific route (Bottleneck Analysis is applied automatically when RouteId is supplied).</summary>
    /// <response code="200">Simulation completed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("simulations")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EgressSimulationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EgressSimulationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RunEgressSimulationAsync([FromBody] RunEgressSimulationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RunEgressSimulationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, request.SiteId,
            request.ZoneId, request.RouteId, request.TwinModelVersion, request.TriggerReason,
            request.SimulatedEvacuationTimeSeconds, request.PredictedPeakOccupancy, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Correlates an egress-simulation run to a SafetyEvents.DrillsAndExercises drill.</summary>
    /// <response code="200">Linked to drill.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("simulations/{egressSimulationRunId:guid}/link-to-drill")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EgressSimulationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EgressSimulationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EgressSimulationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkSimulationToDrillAsync(
        Guid egressSimulationRunId, [FromBody] LinkSimulationToDrillRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new LinkSimulationToDrillCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            egressSimulationRunId, request.DrillId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the most recently started egress-simulation run for a zone.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("zones/{zoneId:guid}/simulations/latest")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLatestEgressSimulationAsync(Guid zoneId, [FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetLatestEgressSimulationQuery(env.TenantId, siteId, zoneId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EvacuationBottlenecks
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Retrieves evacuation bottleneck history, optionally scoped to a route (feeds the bottleneck-recurrence-rate KPI).</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("bottlenecks")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBottleneckHistoryAsync(
        [FromQuery] Guid? siteId, [FromQuery] Guid? routeId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetBottleneckHistoryQuery(env.TenantId, siteId, routeId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
