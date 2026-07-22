// ============================================================================
// File: SafetyProceduresV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing every SafetyProcedures module command
//              (MachineGuardingInspections, RoutineSafetyAuditsIncidentReporting,
//              MaterialHandlingStorage, EmergencyResponsePlanning, ErgonomicsWorkstationDesign,
//              HazardCommunicationRightToKnow, LotoProcedures, PpePolicies) and their read-model
//              "Get" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/safetyprocedures/{sub-component}/...
// - SafetyProcedures commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyProcedures.Contracts.Requests;
using PlantSafety.Application.SafetyProcedures.Commands;
using PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Queries;
using PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Queries;
using PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Queries;
using PlantSafety.Application.SafetyProcedures.LotoProcedures.Queries;
using PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Queries;
using PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Queries;
using PlantSafety.Application.SafetyProcedures.PpePolicies.Queries;
using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Queries;

namespace PlantSafety.Api.SafetyProcedures.Controllers.V1;

/// <summary>V1 controller for the SafetyProcedures module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safetyprocedures")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyProcedures")]
public sealed class SafetyProceduresV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SafetyProceduresV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public SafetyProceduresV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // MachineGuardingInspections
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins a corrective action for a MachineGuardingInspections record.</summary>
    /// <response code="200">Corrective action begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding-inspections/{machineGuardingInspectionsId:guid}/begin-corrective-action")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginCorrectiveActionMachineGuardingInspectionsAsync(Guid machineGuardingInspectionsId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginCorrectiveActionMachineGuardingInspectionsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingInspectionsId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a MachineGuardingInspections record.</summary>
    /// <response code="200">Closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding-inspections/{machineGuardingInspectionsId:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseMachineGuardingInspectionsAsync(
        Guid machineGuardingInspectionsId, [FromBody] CloseMachineGuardingInspectionsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseMachineGuardingInspectionsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingInspectionsId, request.NextDueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Executes an inspection for a MachineGuardingInspections record.</summary>
    /// <response code="200">Inspection executed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("machine-guarding-inspections/{machineGuardingInspectionsId:guid}/execute-inspection")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteInspectionMachineGuardingInspectionsAsync(
        Guid machineGuardingInspectionsId, [FromBody] ExecuteInspectionMachineGuardingInspectionsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ExecuteInspectionMachineGuardingInspectionsCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            machineGuardingInspectionsId, request.DeficiencyCount, request.ComplianceScore, request.InspectedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MachineGuardingInspections read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("machine-guarding-inspections")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingInspectionsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineGuardingInspectionsAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetMachineGuardingInspectionsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // RoutineSafetyAuditsIncidentReporting
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins a corrective action for a RoutineSafetyAuditsIncidentReporting record.</summary>
    /// <response code="200">Corrective action begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("routine-safety-audits-incident-reporting/{routineSafetyAuditsIncidentReportingId:guid}/begin-corrective-action")]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingAsync(
        Guid routineSafetyAuditsIncidentReportingId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            routineSafetyAuditsIncidentReportingId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a RoutineSafetyAuditsIncidentReporting record.</summary>
    /// <response code="200">Closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("routine-safety-audits-incident-reporting/{routineSafetyAuditsIncidentReportingId:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseRoutineSafetyAuditsIncidentReportingAsync(
        Guid routineSafetyAuditsIncidentReportingId, [FromBody] CloseRoutineSafetyAuditsIncidentReportingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseRoutineSafetyAuditsIncidentReportingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            routineSafetyAuditsIncidentReportingId, request.NextDueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Conducts a RoutineSafetyAuditsIncidentReporting audit.</summary>
    /// <response code="200">Audit conducted.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("routine-safety-audits-incident-reporting/{routineSafetyAuditsIncidentReportingId:guid}/conduct")]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConductRoutineSafetyAuditsIncidentReportingAsync(
        Guid routineSafetyAuditsIncidentReportingId, [FromBody] ConductRoutineSafetyAuditsIncidentReportingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ConductRoutineSafetyAuditsIncidentReportingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            routineSafetyAuditsIncidentReportingId, request.FindingCount, request.ComplianceScore, request.ConductedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns RoutineSafetyAuditsIncidentReporting read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("routine-safety-audits-incident-reporting")]
    [ProducesResponseType(typeof(ApiResponse<RoutineSafetyAuditsIncidentReportingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoutineSafetyAuditsIncidentReportingAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetRoutineSafetyAuditsIncidentReportingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // MaterialHandlingStorage
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Flags an obstruction for a MaterialHandlingStorage record.</summary>
    /// <response code="200">Obstruction flagged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("material-handling-storage/{materialHandlingStorageId:guid}/flag-obstruction")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagObstructionMaterialHandlingStorageAsync(
        Guid materialHandlingStorageId, [FromBody] FlagObstructionMaterialHandlingStorageRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new FlagObstructionMaterialHandlingStorageCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            materialHandlingStorageId, request.ObstructionCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a MaterialHandlingStorage record.</summary>
    /// <response code="200">Published.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("material-handling-storage/{materialHandlingStorageId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishMaterialHandlingStorageAsync(
        Guid materialHandlingStorageId, [FromBody] PublishMaterialHandlingStorageRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PublishMaterialHandlingStorageCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            materialHandlingStorageId, request.ApprovedByUserId, request.ReviewCycleDays, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves an obstruction for a MaterialHandlingStorage record.</summary>
    /// <response code="200">Obstruction resolved.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("material-handling-storage/{materialHandlingStorageId:guid}/resolve-obstruction")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveObstructionMaterialHandlingStorageAsync(
        Guid materialHandlingStorageId, [FromBody] ResolveObstructionMaterialHandlingStorageRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ResolveObstructionMaterialHandlingStorageCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            materialHandlingStorageId, request.ComplianceScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a MaterialHandlingStorage record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("material-handling-storage/{materialHandlingStorageId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireMaterialHandlingStorageAsync(Guid materialHandlingStorageId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireMaterialHandlingStorageCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            materialHandlingStorageId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a MaterialHandlingStorage record for approval.</summary>
    /// <response code="200">Submitted for approval.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("material-handling-storage/{materialHandlingStorageId:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApprovalMaterialHandlingStorageAsync(Guid materialHandlingStorageId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitForApprovalMaterialHandlingStorageCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            materialHandlingStorageId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MaterialHandlingStorage read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("material-handling-storage")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMaterialHandlingStorageAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetMaterialHandlingStorageQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // EmergencyResponsePlanning
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Publishes an EmergencyResponsePlanning record.</summary>
    /// <response code="200">Published.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-response-planning/{emergencyResponsePlanningId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishEmergencyResponsePlanningAsync(
        Guid emergencyResponsePlanningId, [FromBody] PublishEmergencyResponsePlanningRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PublishEmergencyResponsePlanningCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyResponsePlanningId, request.ApprovedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a drill for an EmergencyResponsePlanning record.</summary>
    /// <response code="200">Drill recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-response-planning/{emergencyResponsePlanningId:guid}/record-drill")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordDrillEmergencyResponsePlanningAsync(
        Guid emergencyResponsePlanningId, [FromBody] RecordDrillEmergencyResponsePlanningRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordDrillEmergencyResponsePlanningCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyResponsePlanningId, request.ParticipantCount, request.ReadinessScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires an EmergencyResponsePlanning record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-response-planning/{emergencyResponsePlanningId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireEmergencyResponsePlanningAsync(Guid emergencyResponsePlanningId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireEmergencyResponsePlanningCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyResponsePlanningId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a drill for an EmergencyResponsePlanning record.</summary>
    /// <response code="200">Drill scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-response-planning/{emergencyResponsePlanningId:guid}/schedule-drill")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleDrillEmergencyResponsePlanningAsync(
        Guid emergencyResponsePlanningId, [FromBody] ScheduleDrillEmergencyResponsePlanningRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleDrillEmergencyResponsePlanningCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyResponsePlanningId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits an EmergencyResponsePlanning record for approval.</summary>
    /// <response code="200">Submitted for approval.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("emergency-response-planning/{emergencyResponsePlanningId:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApprovalEmergencyResponsePlanningAsync(Guid emergencyResponsePlanningId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitForApprovalEmergencyResponsePlanningCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            emergencyResponsePlanningId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EmergencyResponsePlanning read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("emergency-response-planning")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponsePlanningQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyResponsePlanningAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEmergencyResponsePlanningQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ErgonomicsWorkstationDesign
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Publishes an ErgonomicsWorkstationDesign record.</summary>
    /// <response code="200">Published.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-workstation-design/{ergonomicsWorkstationDesignId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishErgonomicsWorkstationDesignAsync(
        Guid ergonomicsWorkstationDesignId, [FromBody] PublishErgonomicsWorkstationDesignRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PublishErgonomicsWorkstationDesignCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsWorkstationDesignId, request.ApprovedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an assessment for an ErgonomicsWorkstationDesign record.</summary>
    /// <response code="200">Assessment recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-workstation-design/{ergonomicsWorkstationDesignId:guid}/record-assessment")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAssessmentErgonomicsWorkstationDesignAsync(
        Guid ergonomicsWorkstationDesignId, [FromBody] RecordAssessmentErgonomicsWorkstationDesignRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAssessmentErgonomicsWorkstationDesignCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsWorkstationDesignId, request.RiskFactorCount, request.ErgonomicsScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires an ErgonomicsWorkstationDesign record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-workstation-design/{ergonomicsWorkstationDesignId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireErgonomicsWorkstationDesignAsync(Guid ergonomicsWorkstationDesignId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireErgonomicsWorkstationDesignCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsWorkstationDesignId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules an assessment for an ErgonomicsWorkstationDesign record.</summary>
    /// <response code="200">Assessment scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-workstation-design/{ergonomicsWorkstationDesignId:guid}/schedule-assessment")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleAssessmentErgonomicsWorkstationDesignAsync(
        Guid ergonomicsWorkstationDesignId, [FromBody] ScheduleAssessmentErgonomicsWorkstationDesignRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleAssessmentErgonomicsWorkstationDesignCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsWorkstationDesignId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits an ErgonomicsWorkstationDesign record for approval.</summary>
    /// <response code="200">Submitted for approval.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ergonomics-workstation-design/{ergonomicsWorkstationDesignId:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApprovalErgonomicsWorkstationDesignAsync(Guid ergonomicsWorkstationDesignId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitForApprovalErgonomicsWorkstationDesignCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ergonomicsWorkstationDesignId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ErgonomicsWorkstationDesign read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("ergonomics-workstation-design")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsWorkstationDesignQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetErgonomicsWorkstationDesignAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetErgonomicsWorkstationDesignQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HazardCommunicationRightToKnow
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Publishes a HazardCommunicationRightToKnow record.</summary>
    /// <response code="200">Published.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-right-to-know/{hazardCommunicationRightToKnowId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishHazardCommunicationRightToKnowAsync(
        Guid hazardCommunicationRightToKnowId, [FromBody] PublishHazardCommunicationRightToKnowRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PublishHazardCommunicationRightToKnowCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationRightToKnowId, request.ApprovedByUserId, request.LabeledItemCount, request.ReviewCycleDays, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a HazardCommunicationRightToKnow record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-right-to-know/{hazardCommunicationRightToKnowId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireHazardCommunicationRightToKnowAsync(Guid hazardCommunicationRightToKnowId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireHazardCommunicationRightToKnowCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationRightToKnowId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a revision for a HazardCommunicationRightToKnow record.</summary>
    /// <response code="200">Revision scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-right-to-know/{hazardCommunicationRightToKnowId:guid}/schedule-revision")]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleRevisionHazardCommunicationRightToKnowAsync(Guid hazardCommunicationRightToKnowId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleRevisionHazardCommunicationRightToKnowCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationRightToKnowId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a HazardCommunicationRightToKnow record for approval.</summary>
    /// <response code="200">Submitted for approval.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("hazard-communication-right-to-know/{hazardCommunicationRightToKnowId:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApprovalHazardCommunicationRightToKnowAsync(Guid hazardCommunicationRightToKnowId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitForApprovalHazardCommunicationRightToKnowCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardCommunicationRightToKnowId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazardCommunicationRightToKnow read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("hazard-communication-right-to-know")]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationRightToKnowQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardCommunicationRightToKnowAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetHazardCommunicationRightToKnowQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // LotoProcedures
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Publishes a LotoProcedures record.</summary>
    /// <response code="200">Published.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-procedures/{lotoProceduresId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishLotoProceduresAsync(
        Guid lotoProceduresId, [FromBody] PublishLotoProceduresRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PublishLotoProceduresCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProceduresId, request.ApprovedByUserId, request.AuthorizedPersonnelCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the annual audit outcome for a LotoProcedures record.</summary>
    /// <response code="200">Annual audit recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-procedures/{lotoProceduresId:guid}/record-annual-audit")]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAnnualAuditLotoProceduresAsync(
        Guid lotoProceduresId, [FromBody] RecordAnnualAuditLotoProceduresRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordAnnualAuditLotoProceduresCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProceduresId, request.Compliant, request.ComplianceScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a LotoProcedures record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-procedures/{lotoProceduresId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireLotoProceduresAsync(Guid lotoProceduresId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireLotoProceduresCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProceduresId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a revision for a LotoProcedures record.</summary>
    /// <response code="200">Revision scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-procedures/{lotoProceduresId:guid}/schedule-revision")]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleRevisionLotoProceduresAsync(Guid lotoProceduresId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleRevisionLotoProceduresCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProceduresId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a LotoProcedures record for approval.</summary>
    /// <response code="200">Submitted for approval.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("loto-procedures/{lotoProceduresId:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApprovalLotoProceduresAsync(Guid lotoProceduresId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitForApprovalLotoProceduresCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            lotoProceduresId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns LotoProcedures read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("loto-procedures")]
    [ProducesResponseType(typeof(ApiResponse<LotoProceduresQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLotoProceduresAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetLotoProceduresQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PpePolicies
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Publishes a PpePolicies record.</summary>
    /// <response code="200">Published.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe-policies/{ppePoliciesId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishPpePoliciesAsync(
        Guid ppePoliciesId, [FromBody] PublishPpePoliciesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PublishPpePoliciesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppePoliciesId, request.ApprovedByUserId, request.ReplacementScheduleCount, request.ReviewCycleDays, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an enforcement audit for a PpePolicies record.</summary>
    /// <response code="200">Enforcement audit recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe-policies/{ppePoliciesId:guid}/record-enforcement-audit")]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordEnforcementAuditPpePoliciesAsync(
        Guid ppePoliciesId, [FromBody] RecordEnforcementAuditPpePoliciesRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordEnforcementAuditPpePoliciesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppePoliciesId, request.ComplianceScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a PpePolicies record.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe-policies/{ppePoliciesId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetirePpePoliciesAsync(Guid ppePoliciesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetirePpePoliciesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppePoliciesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a revision for a PpePolicies record.</summary>
    /// <response code="200">Revision scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe-policies/{ppePoliciesId:guid}/schedule-revision")]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleRevisionPpePoliciesAsync(Guid ppePoliciesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleRevisionPpePoliciesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppePoliciesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a PpePolicies record for approval.</summary>
    /// <response code="200">Submitted for approval.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("ppe-policies/{ppePoliciesId:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApprovalPpePoliciesAsync(Guid ppePoliciesId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitForApprovalPpePoliciesCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            ppePoliciesId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PpePolicies read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("ppe-policies")]
    [ProducesResponseType(typeof(ApiResponse<PpePoliciesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPpePoliciesAsync([FromQuery] SafetyProceduresQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPpePoliciesQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
