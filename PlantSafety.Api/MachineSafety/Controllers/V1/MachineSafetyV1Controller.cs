// ============================================================================
// File: MachineSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing every MachineSafety module command (LotoSystems,
//              MachineGuardsBarriers, EmergencyStopDevices, PresenceSensingDevices,
//              SafetyValvesRelays, TwoHandControls) and their read-model "Get" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/machinesafety/{sub-component}/...
// - MachineSafety commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.MachineSafety.Contracts.Requests;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.EmergencyStopDevices.Queries;
using PlantSafety.Application.MachineSafety.LotoSystems.Queries;
using PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Queries;
using PlantSafety.Application.MachineSafety.PresenceSensingDevices.Queries;
using PlantSafety.Application.MachineSafety.SafetyValvesRelays.Queries;
using PlantSafety.Application.MachineSafety.TwoHandControls.Queries;

namespace PlantSafety.Api.MachineSafety.Controllers.V1;

/// <summary>V1 controller for the MachineSafety module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/machinesafety")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("MachineSafety")]
public sealed class MachineSafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="MachineSafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public MachineSafetyV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // LotoSystems
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Applies isolation on a LotoSystems record.</summary>
    /// <response code="200">Isolation applied.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-systems/{lotoSystemsId:guid}/apply-isolation")]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyIsolationLotoSystemsAsync(Guid lotoSystemsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ApplyIsolationLotoSystemsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoSystemsId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the annual audit outcome for a LotoSystems record.</summary>
    /// <response code="200">Annual audit recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-systems/{lotoSystemsId:guid}/record-annual-audit")]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAnnualAuditLotoSystemsAsync(
        Guid lotoSystemsId, [FromBody] RecordAnnualAuditLotoSystemsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAnnualAuditLotoSystemsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoSystemsId, request.Compliant, request.ComplianceScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Releases a LotoSystems record.</summary>
    /// <response code="200">Released.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-systems/{lotoSystemsId:guid}/release")]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseLotoSystemsAsync(Guid lotoSystemsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ReleaseLotoSystemsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoSystemsId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies zero energy state for a LotoSystems record.</summary>
    /// <response code="200">Verified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-systems/{lotoSystemsId:guid}/verify-zero-energy")]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyZeroEnergyLotoSystemsAsync(
        Guid lotoSystemsId, [FromBody] VerifyZeroEnergyLotoSystemsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VerifyZeroEnergyLotoSystemsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoSystemsId, request.VerifiedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns LotoSystems read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("loto-systems")]
    [ProducesResponseType(typeof(ApiResponse<LotoSystemsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLotoSystemsAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetLotoSystemsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // MachineGuardsBarriers
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins a corrective action for a MachineGuardsBarriers record.</summary>
    /// <response code="200">Corrective action begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guards-barriers/{machineGuardsBarriersId:guid}/begin-corrective-action")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginCorrectiveActionMachineGuardsBarriersAsync(Guid machineGuardsBarriersId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginCorrectiveActionMachineGuardsBarriersCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardsBarriersId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an inspection result for a MachineGuardsBarriers record.</summary>
    /// <response code="200">Inspection result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guards-barriers/{machineGuardsBarriersId:guid}/record-inspection-result")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordInspectionResultMachineGuardsBarriersAsync(
        Guid machineGuardsBarriersId, [FromBody] RecordInspectionResultMachineGuardsBarriersRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordInspectionResultMachineGuardsBarriersCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardsBarriersId, request.DeficiencyCount, request.GuardIntegrityScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a MachineGuardsBarriers record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guards-barriers/{machineGuardsBarriersId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireMachineGuardsBarriersAsync(Guid machineGuardsBarriersId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireMachineGuardsBarriersCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardsBarriersId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules an inspection for a MachineGuardsBarriers record.</summary>
    /// <response code="200">Inspection scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guards-barriers/{machineGuardsBarriersId:guid}/schedule-inspection")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleInspectionMachineGuardsBarriersAsync(
        Guid machineGuardsBarriersId, [FromBody] ScheduleInspectionMachineGuardsBarriersRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleInspectionMachineGuardsBarriersCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardsBarriersId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies a corrective action for a MachineGuardsBarriers record.</summary>
    /// <response code="200">Corrective action verified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guards-barriers/{machineGuardsBarriersId:guid}/verify-corrective-action")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCorrectiveActionMachineGuardsBarriersAsync(Guid machineGuardsBarriersId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VerifyCorrectiveActionMachineGuardsBarriersCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardsBarriersId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MachineGuardsBarriers read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("machine-guards-barriers")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardsBarriersQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineGuardsBarriersAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetMachineGuardsBarriersQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EmergencyStopDevices
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins a repair for an EmergencyStopDevices record.</summary>
    /// <response code="200">Repair begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-stop-devices/{emergencyStopDevicesId:guid}/begin-repair")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginRepairEmergencyStopDevicesAsync(Guid emergencyStopDevicesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginRepairEmergencyStopDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyStopDevicesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a test result for an EmergencyStopDevices record.</summary>
    /// <response code="200">Test result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-stop-devices/{emergencyStopDevicesId:guid}/record-test-result")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordTestResultEmergencyStopDevicesAsync(
        Guid emergencyStopDevicesId, [FromBody] RecordTestResultEmergencyStopDevicesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordTestResultEmergencyStopDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyStopDevicesId, request.Passed, request.ResponseTimeScore, request.TestedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires an EmergencyStopDevices record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-stop-devices/{emergencyStopDevicesId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireEmergencyStopDevicesAsync(Guid emergencyStopDevicesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireEmergencyStopDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyStopDevicesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a test for an EmergencyStopDevices record.</summary>
    /// <response code="200">Test scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-stop-devices/{emergencyStopDevicesId:guid}/schedule-test")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleTestEmergencyStopDevicesAsync(
        Guid emergencyStopDevicesId, [FromBody] ScheduleTestEmergencyStopDevicesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleTestEmergencyStopDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyStopDevicesId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EmergencyStopDevices read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("emergency-stop-devices")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyStopDevicesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyStopDevicesAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEmergencyStopDevicesQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PresenceSensingDevices
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a calibration result for a PresenceSensingDevices record.</summary>
    /// <response code="200">Calibration result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("presence-sensing-devices/{presenceSensingDevicesId:guid}/record-calibration-result")]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordCalibrationResultPresenceSensingDevicesAsync(
        Guid presenceSensingDevicesId, [FromBody] RecordCalibrationResultPresenceSensingDevicesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordCalibrationResultPresenceSensingDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            presenceSensingDevicesId, request.DetectionAccuracyScore, request.CalibratedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a trigger event for a PresenceSensingDevices record.</summary>
    /// <response code="200">Trigger event recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("presence-sensing-devices/{presenceSensingDevicesId:guid}/record-trigger-event")]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordTriggerEventPresenceSensingDevicesAsync(Guid presenceSensingDevicesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordTriggerEventPresenceSensingDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            presenceSensingDevicesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a PresenceSensingDevices record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("presence-sensing-devices/{presenceSensingDevicesId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetirePresenceSensingDevicesAsync(Guid presenceSensingDevicesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetirePresenceSensingDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            presenceSensingDevicesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a calibration for a PresenceSensingDevices record.</summary>
    /// <response code="200">Calibration scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("presence-sensing-devices/{presenceSensingDevicesId:guid}/schedule-calibration")]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleCalibrationPresenceSensingDevicesAsync(
        Guid presenceSensingDevicesId, [FromBody] ScheduleCalibrationPresenceSensingDevicesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleCalibrationPresenceSensingDevicesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            presenceSensingDevicesId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PresenceSensingDevices read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("presence-sensing-devices")]
    [ProducesResponseType(typeof(ApiResponse<PresenceSensingDevicesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPresenceSensingDevicesAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPresenceSensingDevicesQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SafetyValvesRelays
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins a repair for a SafetyValvesRelays record.</summary>
    /// <response code="200">Repair begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("safety-valves-relays/{safetyValvesRelaysId:guid}/begin-repair")]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginRepairSafetyValvesRelaysAsync(Guid safetyValvesRelaysId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginRepairSafetyValvesRelaysCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            safetyValvesRelaysId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an inspection result for a SafetyValvesRelays record.</summary>
    /// <response code="200">Inspection result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("safety-valves-relays/{safetyValvesRelaysId:guid}/record-inspection-result")]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordInspectionResultSafetyValvesRelaysAsync(
        Guid safetyValvesRelaysId, [FromBody] RecordInspectionResultSafetyValvesRelaysRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordInspectionResultSafetyValvesRelaysCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            safetyValvesRelaysId, request.Reliable, request.ReliabilityScore, request.InspectedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a SafetyValvesRelays record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("safety-valves-relays/{safetyValvesRelaysId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireSafetyValvesRelaysAsync(Guid safetyValvesRelaysId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireSafetyValvesRelaysCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            safetyValvesRelaysId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules an inspection for a SafetyValvesRelays record.</summary>
    /// <response code="200">Inspection scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("safety-valves-relays/{safetyValvesRelaysId:guid}/schedule-inspection")]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleInspectionSafetyValvesRelaysAsync(
        Guid safetyValvesRelaysId, [FromBody] ScheduleInspectionSafetyValvesRelaysRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleInspectionSafetyValvesRelaysCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            safetyValvesRelaysId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns SafetyValvesRelays read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("safety-valves-relays")]
    [ProducesResponseType(typeof(ApiResponse<SafetyValvesRelaysQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSafetyValvesRelaysAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSafetyValvesRelaysQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // TwoHandControls
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a test result for a TwoHandControls record.</summary>
    /// <response code="200">Test result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("two-hand-controls/{twoHandControlsId:guid}/record-test-result")]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordTestResultTwoHandControlsAsync(
        Guid twoHandControlsId, [FromBody] RecordTestResultTwoHandControlsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordTestResultTwoHandControlsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            twoHandControlsId, request.Synchronized, request.SynchronizationScore, request.TestedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a TwoHandControls record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("two-hand-controls/{twoHandControlsId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireTwoHandControlsAsync(Guid twoHandControlsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireTwoHandControlsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            twoHandControlsId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a test for a TwoHandControls record.</summary>
    /// <response code="200">Test scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("two-hand-controls/{twoHandControlsId:guid}/schedule-test")]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleTestTwoHandControlsAsync(
        Guid twoHandControlsId, [FromBody] ScheduleTestTwoHandControlsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleTestTwoHandControlsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            twoHandControlsId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns TwoHandControls read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("two-hand-controls")]
    [ProducesResponseType(typeof(ApiResponse<TwoHandControlsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTwoHandControlsAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetTwoHandControlsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
