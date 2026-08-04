// ============================================================================
// File: WaterDischargeComplianceV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing every EnvironmentalHealth.WaterDischargeCompliance
//              sub-domain command (DischargePermit, DischargePoint, EffluentSample,
//              DischargeLimit, LimitExceedance, DischargeMonitoringReport) and their
//              read-model "Get" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/environmentalhealth/water-discharge-compliance/{sub-entity}/...
// - Commands/queries return PlantSafety.Application.Common.Results.Result<T>, mapped to
//   ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId,
//   [SiteId,] <Entity>Id, ...domain fields..., PerformedByUserId). PerformedByUserId is always
//   populated from the current actor (env.ActorId); the aggregate id is always bound from the
//   route on lifecycle-transition endpoints, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.EnvironmentalHealth.Contracts.Requests;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Queries;

namespace PlantSafety.Api.EnvironmentalHealth.Controllers.V1;

/// <summary>V1 controller for the EnvironmentalHealth.WaterDischargeCompliance sub-domain's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/environmentalhealth/water-discharge-compliance")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EnvironmentalHealth.WaterDischargeCompliance")]
public sealed class WaterDischargeComplianceV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="WaterDischargeComplianceV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public WaterDischargeComplianceV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // DischargePermit
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>Issues a new discharge permit.</summary>
    /// <response code="200">Permit issued.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("discharge-permits")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IssueDischargePermitAsync([FromQuery] Guid? siteId, [FromBody] IssueDischargePermitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new IssueDischargePermitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, siteId,
            request.PermitNumber, request.RegulatoryProgram, request.IssuingAuthority,
            request.EffectiveAtUtc, request.ExpiresAtUtc, request.PermitConditionSummary, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Renews a discharge permit.</summary>
    /// <response code="200">Renewed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-permits/{dischargePermitId:guid}/renew")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenewDischargePermitAsync(Guid dischargePermitId, [FromBody] RenewDischargePermitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RenewDischargePermitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePermitId, request.NewExpiresAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags a discharge permit as expiring soon (Scheduling pre-expiration reminder hook).</summary>
    /// <response code="200">Flagged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-permits/{dischargePermitId:guid}/flag-expiring-soon")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagDischargePermitExpiringSoonAsync(Guid dischargePermitId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new FlagDischargePermitExpiringSoonCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePermitId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends a discharge permit.</summary>
    /// <response code="200">Suspended.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-permits/{dischargePermitId:guid}/suspend")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendDischargePermitAsync(Guid dischargePermitId, [FromBody] SuspendDischargePermitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SuspendDischargePermitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePermitId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Revokes a discharge permit.</summary>
    /// <response code="200">Revoked.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-permits/{dischargePermitId:guid}/revoke")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePermitActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeDischargePermitAsync(Guid dischargePermitId, [FromBody] RevokeDischargePermitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RevokeDischargePermitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePermitId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single DischargePermit record by identifier.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("discharge-permits/{dischargePermitId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetDischargePermitByIdQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDischargePermitByIdAsync(Guid dischargePermitId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetDischargePermitByIdQuery(env.TenantId, dischargePermitId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // DischargePoint
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>Registers a new discharge point.</summary>
    /// <response code="200">Registered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("discharge-points")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterDischargePointAsync([FromQuery] Guid? siteId, [FromBody] RegisterDischargePointRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RegisterDischargePointCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, siteId,
            request.DischargePermitId, request.PointCode, request.PointName, request.ReceivingWaterBody,
            request.Outfall, request.Latitude, request.Longitude, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Deactivates a discharge point.</summary>
    /// <response code="200">Deactivated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-points/{dischargePointId:guid}/deactivate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateDischargePointAsync(Guid dischargePointId, [FromBody] DeactivateDischargePointRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DeactivateDischargePointCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePointId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reactivates a discharge point.</summary>
    /// <response code="200">Reactivated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-points/{dischargePointId:guid}/reactivate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateDischargePointAsync(Guid dischargePointId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ReactivateDischargePointCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePointId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Decommissions a discharge point.</summary>
    /// <response code="200">Decommissioned.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-points/{dischargePointId:guid}/decommission")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargePointActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecommissionDischargePointAsync(Guid dischargePointId, [FromBody] DecommissionDischargePointRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DecommissionDischargePointCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargePointId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists discharge points for a tenant, optionally scoped to a single site.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("discharge-points")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListDischargePointsBySiteQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListDischargePointsBySiteAsync([FromQuery] WaterDischargeComplianceSiteQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListDischargePointsBySiteQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // EffluentSample
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>Records a new effluent sample, or an append-only correction of a prior sample.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("effluent-samples")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EffluentSampleActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EffluentSampleActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordEffluentSampleAsync([FromQuery] Guid? siteId, [FromBody] RecordEffluentSampleRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordEffluentSampleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, siteId,
            request.DischargePointId, request.Parameter, request.MeasuredValue, request.UnitOfMeasure,
            request.SampledAtUtc, request.CollectedByUserId, request.LabSourceSystem, request.ChainOfCustodyReference,
            request.CorrectionOfSampleId, request.CorrectionReason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Voids an effluent sample (chain-of-custody break). The record is retained for audit.</summary>
    /// <response code="200">Voided.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("effluent-samples/{effluentSampleId:guid}/void")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<EffluentSampleActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EffluentSampleActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<EffluentSampleActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoidEffluentSampleAsync(Guid effluentSampleId, [FromBody] VoidEffluentSampleRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VoidEffluentSampleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            effluentSampleId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the sample history for a discharge point within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("discharge-points/{dischargePointId:guid}/effluent-samples")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetSampleHistoryByPointQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSampleHistoryByPointAsync(Guid dischargePointId, [FromQuery] SampleHistoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSampleHistoryByPointQuery(env.TenantId, dischargePointId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // DischargeLimit
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>Defines a new discharge limit for a parameter at a discharge point.</summary>
    /// <response code="200">Defined.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("discharge-limits")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefineDischargeLimitAsync([FromQuery] Guid? siteId, [FromBody] DefineDischargeLimitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DefineDischargeLimitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, siteId,
            request.DischargePointId, request.DischargePermitId, request.Parameter, request.LimitBasis,
            request.LimitValue, request.UnitOfMeasure, request.EffectiveAtUtc, request.PermitConditionReference, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Revises the value of a discharge limit.</summary>
    /// <response code="200">Revised.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-limits/{dischargeLimitId:guid}/revise")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviseDischargeLimitAsync(Guid dischargeLimitId, [FromBody] ReviseDischargeLimitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ReviseDischargeLimitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargeLimitId, request.NewLimitValue, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a discharge limit.</summary>
    /// <response code="200">Retired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-limits/{dischargeLimitId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargeLimitActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetireDischargeLimitAsync(Guid dischargeLimitId, [FromBody] RetireDischargeLimitRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RetireDischargeLimitCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargeLimitId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists discharge limits defined for a discharge point.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("discharge-points/{dischargePointId:guid}/discharge-limits")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListDischargeLimitsByPointQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListDischargeLimitsByPointAsync(Guid dischargePointId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListDischargeLimitsByPointQuery(env.TenantId, dischargePointId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // LimitExceedance
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>Records a new discharge limit exceedance detection.</summary>
    /// <response code="200">Recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("limit-exceedances")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordLimitExceedanceAsync([FromQuery] Guid? siteId, [FromBody] RecordLimitExceedanceRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordLimitExceedanceCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, siteId,
            request.DischargePointId, request.EffluentSampleId, request.DischargeLimitId, request.ParameterName,
            request.MeasuredValue, request.LimitValue, request.Severity, request.DetectedAtUtc,
            request.RepeatExceedanceCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Initiates the exceedance response workflow.</summary>
    /// <response code="200">Response initiated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("limit-exceedances/{limitExceedanceId:guid}/initiate-response")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitiateExceedanceResponseAsync(Guid limitExceedanceId, [FromBody] InitiateExceedanceResponseRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new InitiateExceedanceResponseCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            limitExceedanceId, request.ResponseUserId, request.Notes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes the exceedance response workflow.</summary>
    /// <response code="200">Response closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("limit-exceedances/{limitExceedanceId:guid}/close-response")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<LimitExceedanceActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseExceedanceResponseAsync(Guid limitExceedanceId, [FromBody] CloseExceedanceResponseRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseExceedanceResponseCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            limitExceedanceId, request.ResponseUserId, request.ResolutionNotes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists active (non-Closed) discharge limit exceedances for a tenant/site.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("limit-exceedances/active")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListActiveExceedancesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListActiveExceedancesAsync([FromQuery] WaterDischargeComplianceSiteQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListActiveExceedancesQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // DischargeMonitoringReport
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>Prepares a new Discharge Monitoring Report (DMR) in Draft status.</summary>
    /// <response code="200">Prepared.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("discharge-monitoring-reports")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PrepareDischargeMonitoringReportAsync([FromQuery] Guid? siteId, [FromBody] PrepareDischargeMonitoringReportRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new PrepareDischargeMonitoringReportCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId, siteId,
            request.DischargePermitId, request.ReportingPeriodStartUtc, request.ReportingPeriodEndUtc,
            request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a Discharge Monitoring Report to the regulator.</summary>
    /// <response code="200">Submitted.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-monitoring-reports/{dischargeMonitoringReportId:guid}/submit")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitDischargeMonitoringReportAsync(Guid dischargeMonitoringReportId, [FromBody] SubmitDischargeMonitoringReportRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SubmitDischargeMonitoringReportCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargeMonitoringReportId, request.SubmittedAtUtc, request.SamplesWithinLimitCount,
            request.TotalSamplesCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records regulator acceptance of a Discharge Monitoring Report.</summary>
    /// <response code="200">Accepted.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-monitoring-reports/{dischargeMonitoringReportId:guid}/accept")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptDischargeMonitoringReportAsync(Guid dischargeMonitoringReportId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new AcceptDischargeMonitoringReportCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargeMonitoringReportId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records regulator rejection of a Discharge Monitoring Report.</summary>
    /// <response code="200">Rejected.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("discharge-monitoring-reports/{dischargeMonitoringReportId:guid}/reject")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DischargeMonitoringReportActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectDischargeMonitoringReportAsync(Guid dischargeMonitoringReportId, [FromBody] RejectDischargeMonitoringReportRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RejectDischargeMonitoringReportCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            dischargeMonitoringReportId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Discharge Monitoring Reports overlapping the given period, for a tenant/site.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("discharge-monitoring-reports")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetDischargeMonitoringReportsByPeriodQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDischargeMonitoringReportsByPeriodAsync([FromQuery] DmrPeriodQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetDischargeMonitoringReportsByPeriodQuery(env.TenantId, request.SiteId, request.PeriodStartUtc, request.PeriodEndUtc);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}