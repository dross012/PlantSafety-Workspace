// ============================================================================
// File: ObservabilityV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Application-layer Observability module's Commands
//              and Queries over HTTP (AutomatedIncidentDetection, EnvironmentalAssetMonitoring,
//              FacilityTrafficMonitoring, MachineProcessMonitoring, PPEBehavioralMonitoring).
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/observability/...
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
using PlantSafety.Api.Observability.Contracts.Requests;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.AutomatedIncidentDetection.Queries;
using PlantSafety.Application.Observability.Commands;
using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Queries;
using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Queries;
using PlantSafety.Application.Observability.MachineProcessMonitoring.Queries;
using PlantSafety.Application.Observability.PPEBehavioralMonitoring.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Observability.Controllers.V1;

/// <summary>V1 controller for the Observability module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/observability")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Observability")]
public sealed class ObservabilityV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ObservabilityV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ObservabilityV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ── MachineProcessMonitoring ─────────────────────────────────────────────

    /// <summary>Records a new sensor reading for a MachineProcessMonitoring channel.</summary>
    [HttpPost("machine-process-monitoring/{machineProcessMonitoringId:guid}/record-sensor-reading")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordMachineSensorReadingAsync(
        Guid machineProcessMonitoringId,
        [FromBody] RecordMachineSensorReadingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordMachineSensorReadingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            MachineProcessMonitoringId: machineProcessMonitoringId,
            Reading: request.Reading,
            ThresholdMin: request.ThresholdMin,
            ThresholdMax: request.ThresholdMax,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Raises a process anomaly flag for a MachineProcessMonitoring channel.</summary>
    [HttpPost("machine-process-monitoring/{machineProcessMonitoringId:guid}/raise-anomaly-flag")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RaiseMachineAnomalyFlagAsync(
        Guid machineProcessMonitoringId,
        [FromBody] RaiseMachineAnomalyFlagRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RaiseMachineAnomalyFlagCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            MachineProcessMonitoringId: machineProcessMonitoringId,
            AnomalyDescription: request.AnomalyDescription,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Acknowledges a threshold breach/anomaly flag for a MachineProcessMonitoring channel.</summary>
    [HttpPost("machine-process-monitoring/{machineProcessMonitoringId:guid}/acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcknowledgeMachineProcessMonitoringAsync(
        Guid machineProcessMonitoringId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AcknowledgeMachineProcessMonitoringCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            MachineProcessMonitoringId: machineProcessMonitoringId,
            AcknowledgedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MachineProcessMonitoring read-model data for a tenant/site within a date range.</summary>
    [HttpGet("machine-process-monitoring")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MachineProcessMonitoringQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineProcessMonitoringAsync(
        [FromQuery] ObservabilityListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetMachineProcessMonitoringQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── AutomatedIncidentDetection ───────────────────────────────────────────

    /// <summary>Notifies the assigned supervisor of an AutomatedIncidentDetection.</summary>
    [HttpPost("automated-incident-detection/{automatedIncidentDetectionId:guid}/notify-supervisor")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> NotifySupervisorOfDetectionAsync(
        Guid automatedIncidentDetectionId,
        [FromBody] NotifySupervisorOfDetectionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new NotifySupervisorOfDetectionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AutomatedIncidentDetectionId: automatedIncidentDetectionId,
            SupervisorUserId: request.SupervisorUserId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Acknowledges an AutomatedIncidentDetection and begins evidence review.</summary>
    [HttpPost("automated-incident-detection/{automatedIncidentDetectionId:guid}/acknowledge")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcknowledgeDetectionAsync(
        Guid automatedIncidentDetectionId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AcknowledgeDetectionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AutomatedIncidentDetectionId: automatedIncidentDetectionId,
            AcknowledgedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Escalates a confirmed AutomatedIncidentDetection into a formal Incident Management record.</summary>
    [HttpPost("automated-incident-detection/{automatedIncidentDetectionId:guid}/escalate-to-incident")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EscalateDetectionToIncidentAsync(
        Guid automatedIncidentDetectionId,
        [FromBody] EscalateDetectionToIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EscalateDetectionToIncidentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AutomatedIncidentDetectionId: automatedIncidentDetectionId,
            IncidentReferenceId: request.IncidentReferenceId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Dismisses an AutomatedIncidentDetection as a false positive.</summary>
    [HttpPost("automated-incident-detection/{automatedIncidentDetectionId:guid}/dismiss")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DismissDetectionAsync(
        Guid automatedIncidentDetectionId,
        [FromBody] DismissDetectionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DismissDetectionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AutomatedIncidentDetectionId: automatedIncidentDetectionId,
            Reason: request.Reason,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns AutomatedIncidentDetection read-model data for a tenant/site within a date range.</summary>
    [HttpGet("automated-incident-detection")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AutomatedIncidentDetectionQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAutomatedIncidentDetectionAsync(
        [FromQuery] ObservabilityListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAutomatedIncidentDetectionQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── EnvironmentalAssetMonitoring ─────────────────────────────────────────

    /// <summary>Returns EnvironmentalAssetMonitoring read-model data for a tenant/site within a date range.</summary>
    [HttpGet("environmental-asset-monitoring")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalAssetMonitoringQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalAssetMonitoringQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnvironmentalAssetMonitoringAsync(
        [FromQuery] ObservabilityListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEnvironmentalAssetMonitoringQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── FacilityTrafficMonitoring ────────────────────────────────────────────

    /// <summary>Returns FacilityTrafficMonitoring read-model data for a tenant/site within a date range.</summary>
    [HttpGet("facility-traffic-monitoring")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<FacilityTrafficMonitoringQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FacilityTrafficMonitoringQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFacilityTrafficMonitoringAsync(
        [FromQuery] ObservabilityListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetFacilityTrafficMonitoringQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PPEBehavioralMonitoring ──────────────────────────────────────────────

    /// <summary>Returns PPEBehavioralMonitoring read-model data for a tenant/site within a date range.</summary>
    [HttpGet("ppe-behavioral-monitoring")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PPEBehavioralMonitoringQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEBehavioralMonitoringQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPPEBehavioralMonitoringAsync(
        [FromQuery] ObservabilityListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPPEBehavioralMonitoringQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Bulk ─────────────────────────────────────────────────────────────────

    /// <summary>Applies a single operation uniformly across multiple Observability resources.</summary>
    [HttpPost("bulk")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateObservabilityResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateObservabilityResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateAsync(
        [FromBody] BulkUpdateObservabilityRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new BulkUpdateObservabilityCommand(
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
