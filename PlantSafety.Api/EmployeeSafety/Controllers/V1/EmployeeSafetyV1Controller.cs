// ============================================================================
// File: EmployeeSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing every EmployeeSafety module command (LotoPrograms,
//              EnvironmentalHealthMonitoring, ErgonomicsAndMaterialHandling,
//              HazardCommunicationTraining, MachineGuarding, Ppe, EmergencyPreparedness)
//              and their read-model "Get" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/employeesafety/{sub-component}/...
// - EmployeeSafety commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.EmployeeSafety.Contracts.Requests;
using PlantSafety.Application.EmployeeSafety.Commands;
using PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Queries;
using PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Queries;
using PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Queries;
using PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Queries;
using PlantSafety.Application.EmployeeSafety.LotoPrograms.Queries;
using PlantSafety.Application.EmployeeSafety.MachineGuarding.Queries;
using PlantSafety.Application.EmployeeSafety.Ppe.Queries;

namespace PlantSafety.Api.EmployeeSafety.Controllers.V1;

/// <summary>V1 controller for the EmployeeSafety module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/employeesafety")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EmployeeSafety")]
public sealed class EmployeeSafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EmployeeSafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public EmployeeSafetyV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // LotoPrograms
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Applies isolation on a LotoPrograms record.</summary>
    /// <response code="200">Isolation applied.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">LotoPrograms record not found.</response>
    [HttpPost("loto-programs/{lotoProgramsId:guid}/apply-isolation")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyIsolationLotoProgramsAsync(
        Guid lotoProgramsId, [FromBody] ApplyIsolationLotoProgramsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ApplyIsolationLotoProgramsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProgramsId, request.LocksAppliedCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the annual audit outcome for a LotoPrograms record.</summary>
    /// <response code="200">Annual audit recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">LotoPrograms record not found.</response>
    [HttpPost("loto-programs/{lotoProgramsId:guid}/record-annual-audit")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAnnualAuditLotoProgramsAsync(
        Guid lotoProgramsId, [FromBody] RecordAnnualAuditLotoProgramsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAnnualAuditLotoProgramsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProgramsId, request.Compliant, request.ComplianceScore, request.Notes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Releases a LotoPrograms record.</summary>
    /// <response code="200">Released.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">LotoPrograms record not found.</response>
    [HttpPost("loto-programs/{lotoProgramsId:guid}/release")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseLotoProgramsAsync(
        Guid lotoProgramsId, [FromBody] ReleaseLotoProgramsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ReleaseLotoProgramsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProgramsId, request.ReleasedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies zero energy state for a LotoPrograms record.</summary>
    /// <response code="200">Verified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">LotoPrograms record not found.</response>
    [HttpPost("loto-programs/{lotoProgramsId:guid}/verify-zero-energy")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyZeroEnergyLotoProgramsAsync(
        Guid lotoProgramsId, [FromBody] VerifyZeroEnergyLotoProgramsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VerifyZeroEnergyLotoProgramsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProgramsId, request.VerifiedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns LotoPrograms read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("loto-programs")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<LotoProgramsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLotoProgramsAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetLotoProgramsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EnvironmentalHealthMonitoring
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins a corrective action for an EnvironmentalHealthMonitoring record.</summary>
    /// <response code="200">Corrective action begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("environmental-health-monitoring/{environmentalHealthMonitoringId:guid}/begin-corrective-action")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginCorrectiveActionEnvironmentalHealthMonitoringAsync(
        Guid environmentalHealthMonitoringId, [FromBody] BeginCorrectiveActionEnvironmentalHealthMonitoringRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginCorrectiveActionEnvironmentalHealthMonitoringCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            environmentalHealthMonitoringId, request.Description, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a measurement for an EnvironmentalHealthMonitoring record.</summary>
    /// <response code="200">Measurement recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("environmental-health-monitoring/{environmentalHealthMonitoringId:guid}/record-measurement")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordMeasurementEnvironmentalHealthMonitoringAsync(
        Guid environmentalHealthMonitoringId, [FromBody] RecordMeasurementEnvironmentalHealthMonitoringRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordMeasurementEnvironmentalHealthMonitoringCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            environmentalHealthMonitoringId, request.ExposureLevelScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves an exceedance for an EnvironmentalHealthMonitoring record.</summary>
    /// <response code="200">Exceedance resolved.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("environmental-health-monitoring/{environmentalHealthMonitoringId:guid}/resolve-exceedance")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveExceedanceEnvironmentalHealthMonitoringAsync(
        Guid environmentalHealthMonitoringId, [FromBody] ResolveExceedanceEnvironmentalHealthMonitoringRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ResolveExceedanceEnvironmentalHealthMonitoringCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            environmentalHealthMonitoringId, request.ReMeasuredScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EnvironmentalHealthMonitoring read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("environmental-health-monitoring")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalHealthMonitoringQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnvironmentalHealthMonitoringAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEnvironmentalHealthMonitoringQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ErgonomicsAndMaterialHandling
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Closes an ErgonomicsAndMaterialHandling record.</summary>
    /// <response code="200">Closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-and-material-handling/{ergonomicsAndMaterialHandlingId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseErgonomicsAndMaterialHandlingAsync(
        Guid ergonomicsAndMaterialHandlingId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseErgonomicsAndMaterialHandlingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsAndMaterialHandlingId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records implemented controls for an ErgonomicsAndMaterialHandling record.</summary>
    /// <response code="200">Controls recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-and-material-handling/{ergonomicsAndMaterialHandlingId:guid}/implement-controls")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImplementControlsErgonomicsAndMaterialHandlingAsync(
        Guid ergonomicsAndMaterialHandlingId, [FromBody] ImplementControlsErgonomicsAndMaterialHandlingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ImplementControlsErgonomicsAndMaterialHandlingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsAndMaterialHandlingId, request.ControlsDescription, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a risk assessment for an ErgonomicsAndMaterialHandling record.</summary>
    /// <response code="200">Assessment recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-and-material-handling/{ergonomicsAndMaterialHandlingId:guid}/record-assessment")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAssessmentErgonomicsAndMaterialHandlingAsync(
        Guid ergonomicsAndMaterialHandlingId, [FromBody] RecordAssessmentErgonomicsAndMaterialHandlingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAssessmentErgonomicsAndMaterialHandlingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsAndMaterialHandlingId, request.RiskFactorCount, request.RiskScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a follow-up for an ErgonomicsAndMaterialHandling record.</summary>
    /// <response code="200">Follow-up scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-and-material-handling/{ergonomicsAndMaterialHandlingId:guid}/schedule-follow-up")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleFollowUpErgonomicsAndMaterialHandlingAsync(
        Guid ergonomicsAndMaterialHandlingId, [FromBody] ScheduleFollowUpErgonomicsAndMaterialHandlingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleFollowUpErgonomicsAndMaterialHandlingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsAndMaterialHandlingId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ErgonomicsAndMaterialHandling read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("ergonomics-and-material-handling")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsAndMaterialHandlingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetErgonomicsAndMaterialHandlingAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetErgonomicsAndMaterialHandlingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HazardCommunicationTraining
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Closes a HazardCommunicationTraining record.</summary>
    /// <response code="200">Closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-training/{hazardCommunicationTrainingId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseHazardCommunicationTrainingAsync(
        Guid hazardCommunicationTrainingId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseHazardCommunicationTrainingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationTrainingId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Delivers training for a HazardCommunicationTraining record.</summary>
    /// <response code="200">Training delivered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-training/{hazardCommunicationTrainingId:guid}/deliver-training")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeliverTrainingHazardCommunicationTrainingAsync(
        Guid hazardCommunicationTrainingId, [FromBody] DeliverTrainingHazardCommunicationTrainingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DeliverTrainingHazardCommunicationTrainingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationTrainingId, request.EmployeesTrainedCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules the next training cycle for a HazardCommunicationTraining record.</summary>
    /// <response code="200">Next cycle scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-training/{hazardCommunicationTrainingId:guid}/schedule-next-cycle")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleNextCycleHazardCommunicationTrainingAsync(
        Guid hazardCommunicationTrainingId, [FromBody] ScheduleNextCycleHazardCommunicationTrainingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleNextCycleHazardCommunicationTrainingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationTrainingId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies comprehension for a HazardCommunicationTraining record.</summary>
    /// <response code="200">Comprehension verified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-training/{hazardCommunicationTrainingId:guid}/verify-comprehension")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyComprehensionHazardCommunicationTrainingAsync(
        Guid hazardCommunicationTrainingId, [FromBody] VerifyComprehensionHazardCommunicationTrainingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VerifyComprehensionHazardCommunicationTrainingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationTrainingId, request.ComprehensionScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazardCommunicationTraining read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("hazard-communication-training")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationTrainingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardCommunicationTrainingAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetHazardCommunicationTrainingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // MachineGuarding
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Flags a bypass on a MachineGuarding record.</summary>
    /// <response code="200">Bypass flagged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding/{machineGuardingId:guid}/flag-bypass")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagBypassMachineGuardingAsync(
        Guid machineGuardingId, [FromBody] FlagBypassMachineGuardingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new FlagBypassMachineGuardingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingId, request.FindingNotes, request.DetectedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an inspection result for a MachineGuarding record.</summary>
    /// <response code="200">Inspection result recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding/{machineGuardingId:guid}/record-inspection-result")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordInspectionResultMachineGuardingAsync(
        Guid machineGuardingId, [FromBody] RecordInspectionResultMachineGuardingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordInspectionResultMachineGuardingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingId, request.Passed, request.DeficiencyCount, request.ConditionScore,
            request.InspectedByUserId, request.Notes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a MachineGuarding record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding/{machineGuardingId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireMachineGuardingAsync(
        Guid machineGuardingId, [FromBody] RetireMachineGuardingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireMachineGuardingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a corrective action for a MachineGuarding record.</summary>
    /// <response code="200">Corrective action scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding/{machineGuardingId:guid}/schedule-corrective-action")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleCorrectiveActionMachineGuardingAsync(
        Guid machineGuardingId, [FromBody] ScheduleCorrectiveActionMachineGuardingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleCorrectiveActionMachineGuardingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules an inspection for a MachineGuarding record.</summary>
    /// <response code="200">Inspection scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding/{machineGuardingId:guid}/schedule-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleInspectionMachineGuardingAsync(
        Guid machineGuardingId, [FromBody] ScheduleInspectionMachineGuardingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleInspectionMachineGuardingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies a corrective action for a MachineGuarding record.</summary>
    /// <response code="200">Corrective action verified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding/{machineGuardingId:guid}/verify-corrective-action")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCorrectiveActionMachineGuardingAsync(
        Guid machineGuardingId, [FromBody] VerifyCorrectiveActionMachineGuardingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VerifyCorrectiveActionMachineGuardingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingId, request.VerifiedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MachineGuarding read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("machine-guarding")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineGuardingAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetMachineGuardingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Ppe
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Flags a Ppe record for replacement.</summary>
    /// <response code="200">Flagged for replacement.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe/{ppeId:guid}/flag-for-replacement")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagForReplacementPpeAsync(
        Guid ppeId, [FromBody] FlagForReplacementPpeRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new FlagForReplacementPpeCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppeId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a fit test result for a Ppe record.</summary>
    /// <response code="200">Fit test recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe/{ppeId:guid}/record-fit-test")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordFitTestPpeAsync(
        Guid ppeId, [FromBody] RecordFitTestPpeRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordFitTestPpeCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppeId, request.Passed, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an inspection for a Ppe record.</summary>
    /// <response code="200">Inspection recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe/{ppeId:guid}/record-inspection")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordInspectionPpeAsync(
        Guid ppeId, [FromBody] RecordInspectionPpeRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordInspectionPpeCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppeId, request.ConditionScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a Ppe record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe/{ppeId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpeActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetirePpeAsync(Guid ppeId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetirePpeCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppeId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Ppe read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("ppe")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PpeQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPpeAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPpeQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EmergencyPreparedness
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Conducts a drill for an EmergencyPreparedness record.</summary>
    /// <response code="200">Drill conducted.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-preparedness/{emergencyPreparednessId:guid}/conduct-drill")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConductDrillEmergencyPreparednessAsync(
        Guid emergencyPreparednessId, [FromBody] ConductDrillEmergencyPreparednessRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ConductDrillEmergencyPreparednessCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyPreparednessId, request.ParticipantCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records readiness for an EmergencyPreparedness record.</summary>
    /// <response code="200">Readiness recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-preparedness/{emergencyPreparednessId:guid}/record-readiness")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordReadinessEmergencyPreparednessAsync(
        Guid emergencyPreparednessId, [FromBody] RecordReadinessEmergencyPreparednessRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordReadinessEmergencyPreparednessCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyPreparednessId, request.ReadinessScore, request.GapNotes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a drill for an EmergencyPreparedness record.</summary>
    /// <response code="200">Drill scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-preparedness/{emergencyPreparednessId:guid}/schedule-drill")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleDrillEmergencyPreparednessAsync(
        Guid emergencyPreparednessId, [FromBody] ScheduleDrillEmergencyPreparednessRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleDrillEmergencyPreparednessCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyPreparednessId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EmergencyPreparedness read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("emergency-preparedness")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyPreparednessAsync([FromQuery] EmployeeSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEmergencyPreparednessQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
