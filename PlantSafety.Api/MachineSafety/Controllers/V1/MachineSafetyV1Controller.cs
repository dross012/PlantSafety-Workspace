// ============================================================================
// File: MachineSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing every MachineSafety module command (LotoSystems,
//              MachineGuardsBarriers, EmergencyStopDevices, PresenceSensingDevices,
//              SafetyValvesRelays, TwoHandControls, CraneHoistSystems) and their read-model
//              "Get" queries over HTTP.
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
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Approve)]
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
    [Authorize(Policy = SafetyTierPolicies.Read)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Approve)]
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
    [Authorize(Policy = SafetyTierPolicies.Read)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Read)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Read)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Read)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Write)]
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
    [Authorize(Policy = SafetyTierPolicies.Read)]
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

    // ══════════════════════════════════════════════════════════════════════
    // CraneHoistSystems
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Registers a new crane/hoist asset in the fleet.</summary>
    /// <response code="200">Asset registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("crane-hoist-systems")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterCraneHoistAssetAsync([FromBody] RegisterCraneHoistAssetRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterCraneHoistAssetCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AssetTag, request.AssetTitle, request.AssetType, request.RatedLoadCapacityKg, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a lift event for a CraneHoistSystems asset.</summary>
    /// <response code="200">Lift event recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/record-lift-event")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordLiftEventAsync(
        Guid craneHoistSystemsId, [FromBody] RecordLiftEventRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordLiftEventCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.ObservedLoadKg, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an overload event for a CraneHoistSystems asset.</summary>
    /// <response code="200">Overload event recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/record-overload-event")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordOverloadEventAsync(
        Guid craneHoistSystemsId, [FromBody] RecordOverloadEventRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordOverloadEventCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.ObservedLoadKg, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules the next mandatory inspection for a CraneHoistSystems asset.</summary>
    /// <response code="200">Inspection scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/schedule-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleCraneInspectionAsync(
        Guid craneHoistSystemsId, [FromBody] ScheduleCraneInspectionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleCraneInspectionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the outcome of a completed crane inspection.</summary>
    /// <response code="200">Inspection completed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/complete-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCraneInspectionAsync(
        Guid craneHoistSystemsId, [FromBody] CompleteCraneInspectionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CompleteCraneInspectionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.Passed, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Qualifies an operator to run a CraneHoistSystems asset.</summary>
    /// <response code="200">Operator qualified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/qualify-operator")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QualifyOperatorAsync(
        Guid craneHoistSystemsId, [FromBody] QualifyOperatorRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new QualifyOperatorCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.OperatorUserId, request.ExpiresAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends the current operator's qualification on a CraneHoistSystems asset.</summary>
    /// <response code="200">Qualification suspended.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/suspend-operator-qualification")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendOperatorQualificationAsync(
        Guid craneHoistSystemsId, [FromBody] SuspendOperatorQualificationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SuspendOperatorQualificationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a rigging safety inspection for a CraneHoistSystems asset.</summary>
    /// <response code="200">Rigging inspection recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("crane-hoist-systems/{craneHoistSystemsId:guid}/record-rigging-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordRiggingInspectionAsync(
        Guid craneHoistSystemsId, [FromBody] RecordRiggingInspectionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordRiggingInspectionCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            craneHoistSystemsId, request.DefectDetected, request.Notes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CraneHoistSystems read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("crane-hoist-systems")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CraneHoistSystemsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCraneHoistSystemsAsync([FromQuery] MachineSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCraneHoistSystemsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single CraneHoistSystems asset by identifier.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("crane-hoist-systems/{craneHoistSystemsId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.CraneHoistSystemsItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCraneHoistAssetByIdAsync(Guid craneHoistSystemsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCraneHoistAssetByIdQuery(env.TenantId, craneHoistSystemsId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the recorded lift events for a single CraneHoistSystems asset.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("crane-hoist-systems/{craneHoistSystemsId:guid}/lift-events")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.CraneHoistSystemsItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListLiftEventsByAssetAsync(Guid craneHoistSystemsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListLiftEventsByAssetQuery(env.TenantId, craneHoistSystemsId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the inspection history for a single CraneHoistSystems asset.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("crane-hoist-systems/{craneHoistSystemsId:guid}/inspection-history")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.CraneHoistSystemsItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInspectionHistoryAsync(Guid craneHoistSystemsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetInspectionHistoryQuery(env.TenantId, craneHoistSystemsId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CraneHoistSystems assets due (or overdue) for inspection as of a given UTC instant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("crane-hoist-systems/due-for-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.CraneHoistSystemsItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAssetsDueForInspectionAsync([FromQuery] ListAssetsDueForInspectionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListAssetsDueForInspectionQuery(env.TenantId, request.SiteId, request.AsOfUtc);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns CraneHoistSystems assets whose operator qualification expires within a window.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("crane-hoist-systems/expiring-operator-qualifications")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.CraneHoistSystemsItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListExpiringOperatorQualificationsAsync([FromQuery] ListExpiringOperatorQualificationsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListExpiringOperatorQualificationsQuery(env.TenantId, request.SiteId, request.WithinDays);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
