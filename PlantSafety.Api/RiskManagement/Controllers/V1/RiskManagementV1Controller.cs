// ============================================================================
// File: RiskManagementV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing every RiskManagement module command (identify +
//              7 Full Risk Management Process lifecycle commands) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Envelope fields are populated via RequestEnvelopeFactory, mirroring the canonical
//   Features/Compliance/Permits pattern. Commands are dispatched through MediatR ISender.
// - RiskAssessmentCommands.cs commands implement MediatR IRequest<Result<T>> directly (not the
//   ICommand<T> marker) and therefore carry no ApiVersionToken/ActorId fields; the "*By" field on
//   each command IS the acting user and is populated from the envelope ActorId.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.RiskManagement.Contracts.Requests;
using PlantSafety.Application.RiskManagement.Commands;

namespace PlantSafety.Api.RiskManagement.Controllers.V1;

/// <summary>V1 controller for RiskManagement identify + Full Risk Management Process commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/riskmanagement")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("RiskManagement")]
public sealed class RiskManagementV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RiskManagementV1Controller"/>.</summary>
    public RiskManagementV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Identifies a new hazard/risk (Phase 1, Step 1).</summary>
    /// <response code="200">Risk identified successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateRiskResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CreateRiskResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateRiskRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = CreateRiskCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            riskTitle: request.RiskTitle,
            location: request.Location,
            hazardSource: request.HazardSource,
            ownerId: request.OwnerId,
            reviewRequired: request.ReviewRequired,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken) with { ActorId = env.ActorId };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the Severity x Likelihood risk score for a hazard (Phase 1, Step 2).</summary>
    /// <response code="200">Risk scored successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/score")]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ScoreAsync(Guid riskId, [FromBody] ScoreRiskRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ScoreRiskCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, request.Severity, request.Likelihood, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Assigns a P1-P4 remediation priority and documents the finding (Phase 1, Steps 3-4).</summary>
    /// <response code="200">Risk prioritized successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/prioritize")]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PrioritizeAsync(Guid riskId, [FromBody] PrioritizeRiskRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PrioritizeRiskCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, request.Priority, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Applies a control measure from the Hierarchy of Controls (Phase 2, Steps 5-6).</summary>
    /// <response code="200">Control measure applied successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/control-measures")]
    [ProducesResponseType(typeof(ApiResponse<ApplyRiskControlMeasureResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ApplyRiskControlMeasureResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApplyControlMeasureAsync(Guid riskId, [FromBody] ApplyRiskControlMeasureRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApplyRiskControlMeasureCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, request.HierarchyLevel, request.Description, request.Justification, request.ResponsiblePersonId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Validates residual risk after controls are applied (Phase 2, Step 7).</summary>
    /// <response code="200">Effectiveness validated successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/validate-effectiveness")]
    [ProducesResponseType(typeof(ApiResponse<ValidateRiskEffectivenessResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ValidateRiskEffectivenessResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ValidateEffectivenessAsync(Guid riskId, [FromBody] ValidateRiskEffectivenessRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ValidateRiskEffectivenessCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, request.ResidualSeverity, request.ResidualLikelihood, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts the periodic monitoring/review cycle for a Mitigated risk (Phase 3, Steps 9-10).</summary>
    /// <response code="200">Review cycle started successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/start-review")]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartReviewCycleAsync(Guid riskId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartRiskReviewCycleCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a periodic review cycle, bumping the revision and returning the risk to Mitigated.</summary>
    /// <response code="200">Review completed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/complete-review")]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteReviewAsync(Guid riskId, [FromBody] CompleteRiskReviewRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteRiskReviewCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, request.NewReviewCadence, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a Mitigated or UnderReview risk.</summary>
    /// <response code="200">Risk closed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{riskId:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RiskStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid riskId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseRiskCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, riskId, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
