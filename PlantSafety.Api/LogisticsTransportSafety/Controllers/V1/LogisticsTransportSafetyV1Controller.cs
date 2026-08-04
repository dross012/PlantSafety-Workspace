// ============================================================================
// File: LogisticsTransportSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing every LogisticsTransportSafety module command (TransportVehicles,
//              Carriers, DriverQualifications, TransportTrips, LoadSecurementChecks, TransportIncidents)
//              and their read-model "Get"/"List" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/logisticstransportsafety/{sub-component}/...
// - LogisticsTransportSafety commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.LogisticsTransportSafety.Contracts.Requests;
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Queries;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Queries;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Queries;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Queries;

namespace PlantSafety.Api.LogisticsTransportSafety.Controllers.V1;

/// <summary>V1 controller for the LogisticsTransportSafety module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/logisticstransportsafety")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("LogisticsTransportSafety")]
public sealed class LogisticsTransportSafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="LogisticsTransportSafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public LogisticsTransportSafetyV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // TransportVehicles
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new transport vehicle.</summary>
    /// <response code="200">Registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("transport-vehicles")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterTransportVehicleAsync([FromBody] RegisterTransportVehicleRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterTransportVehicleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.VehicleTitle, request.VehicleType, request.VehicleIdentifier, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules an inspection for a TransportVehicle record.</summary>
    /// <response code="200">Inspection scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-vehicles/{transportVehicleId:guid}/schedule-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleInspectionTransportVehicleAsync(
        Guid transportVehicleId, [FromBody] ScheduleInspectionTransportVehicleRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleInspectionTransportVehicleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportVehicleId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an inspection result for a TransportVehicle record.</summary>
    /// <response code="200">Inspection result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-vehicles/{transportVehicleId:guid}/record-inspection-result")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordInspectionResultTransportVehicleAsync(
        Guid transportVehicleId, [FromBody] RecordInspectionResultTransportVehicleRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordInspectionResultTransportVehicleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportVehicleId, request.Passed, request.InspectionScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a TransportVehicle record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-vehicles/{transportVehicleId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireTransportVehicleAsync(Guid transportVehicleId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireTransportVehicleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportVehicleId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single TransportVehicle record by identity.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("transport-vehicles/{transportVehicleId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<TransportVehicleQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransportVehicleByIdAsync(Guid transportVehicleId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetTransportVehicleByIdQuery(env.TenantId, transportVehicleId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Carriers
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new fleet or third-party transport carrier.</summary>
    /// <response code="200">Registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("carriers")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterCarrierAsync([FromBody] RegisterCarrierRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterCarrierCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.CarrierName, request.CarrierType, request.InsuranceExpiryAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Qualifies a Carrier record to perform transport work.</summary>
    /// <response code="200">Qualified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("carriers/{carrierId:guid}/qualify")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QualifyCarrierAsync(Guid carrierId, [FromBody] QualifyCarrierRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new QualifyCarrierCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            carrierId, request.SafetyRatingScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends a Carrier record.</summary>
    /// <response code="200">Suspended.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("carriers/{carrierId:guid}/suspend")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendCarrierAsync(Guid carrierId, [FromBody] SuspendCarrierRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SuspendCarrierCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            carrierId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a Carrier record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("carriers/{carrierId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CarrierActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireCarrierAsync(Guid carrierId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireCarrierCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            carrierId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Carrier read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("carriers")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CarrierQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCarriersAsync([FromQuery] LogisticsTransportSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCarriersQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // DriverQualifications
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Qualifies a driver for on-site transport work with a specific credential type.</summary>
    /// <response code="200">Qualified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("driver-qualifications")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> QualifyDriverAsync([FromBody] QualifyDriverRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new QualifyDriverCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.DriverUserId, request.QualificationType, request.LicenseNumber,
            request.IssuedAtUtc, request.ExpiresAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends a DriverQualification record.</summary>
    /// <response code="200">Suspended.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("driver-qualifications/{driverQualificationId:guid}/suspend")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendDriverQualificationAsync(
        Guid driverQualificationId, [FromBody] SuspendDriverQualificationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SuspendDriverQualificationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            driverQualificationId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reinstates a suspended DriverQualification record.</summary>
    /// <response code="200">Reinstated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("driver-qualifications/{driverQualificationId:guid}/reinstate")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReinstateDriverQualificationAsync(Guid driverQualificationId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ReinstateDriverQualificationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            driverQualificationId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a DriverQualification record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("driver-qualifications/{driverQualificationId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireDriverQualificationAsync(Guid driverQualificationId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireDriverQualificationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            driverQualificationId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns current qualification status/history for a single driver.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("driver-qualifications/drivers/{driverUserId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<DriverQualificationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDriverQualificationStatusAsync(Guid driverUserId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetDriverQualificationStatusQuery(env.TenantId, driverUserId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // TransportTrips
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Plans a new material/personnel transport trip.</summary>
    /// <response code="200">Planned.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("transport-trips")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PlanTransportTripAsync([FromBody] PlanTransportTripRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PlanTransportTripCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.VehicleId, request.DriverUserId, request.TripType, request.RouteDescription,
            request.PlannedDepartureAtUtc, request.PlannedArrivalAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a TransportTrip record.</summary>
    /// <response code="200">Completed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-trips/{transportTripId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteTransportTripAsync(
        Guid transportTripId, [FromBody] CompleteTransportTripRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CompleteTransportTripCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportTripId, request.ActualDepartureAtUtc, request.ActualArrivalAtUtc, request.DistanceMiles, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Cancels a TransportTrip record before completion.</summary>
    /// <response code="200">Cancelled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-trips/{transportTripId:guid}/cancel")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelTransportTripAsync(Guid transportTripId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CancelTransportTripCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportTripId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns all active (Planned or InProgress) TransportTrip records.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("transport-trips/active")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<TransportTripQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveTripsAsync([FromQuery] LogisticsTransportSafetyActiveRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListActiveTripsQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // LoadSecurementChecks
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records the outcome of a load securement check.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("load-securement-checks")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LoadSecurementCheckActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoadSecurementCheckActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordLoadSecurementCheckAsync([FromBody] RecordLoadSecurementCheckRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordLoadSecurementCheckCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.TripId, request.VehicleId, request.CheckedByUserId, request.Passed,
            request.DeficiencyCount, request.SecurementScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks corrective action for a failed LoadSecurementCheck as completed.</summary>
    /// <response code="200">Corrective action completed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("load-securement-checks/{loadSecurementCheckId:guid}/complete-corrective-action")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LoadSecurementCheckActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoadSecurementCheckActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LoadSecurementCheckActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCorrectiveActionLoadSecurementCheckAsync(Guid loadSecurementCheckId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CompleteCorrectiveActionLoadSecurementCheckCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            loadSecurementCheckId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns failed LoadSecurementCheck records for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("load-securement-checks/failures")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<LoadSecurementCheckQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListLoadSecurementFailuresAsync([FromQuery] LogisticsTransportSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListLoadSecurementFailuresQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // TransportIncidents
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a new transport incident.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("transport-incidents")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordTransportIncidentAsync([FromBody] RecordTransportIncidentRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordTransportIncidentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.TripId, request.VehicleId, request.DriverUserId, request.IncidentType,
            request.Severity, request.Location, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins investigation of a TransportIncident record.</summary>
    /// <response code="200">Investigation begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-incidents/{transportIncidentId:guid}/begin-investigation")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginInvestigationTransportIncidentAsync(Guid transportIncidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginInvestigationTransportIncidentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportIncidentId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a TransportIncident investigation.</summary>
    /// <response code="200">Closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("transport-incidents/{transportIncidentId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseTransportIncidentAsync(Guid transportIncidentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseTransportIncidentCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            transportIncidentId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns TransportIncident history for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("transport-incidents")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<TransportIncidentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransportIncidentHistoryAsync([FromQuery] LogisticsTransportSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetTransportIncidentHistoryQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
