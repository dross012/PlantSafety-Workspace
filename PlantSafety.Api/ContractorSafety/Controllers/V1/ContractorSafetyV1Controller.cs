// ============================================================================
// File: ContractorSafetyV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing every ContractorSafety module command
//              (PreQualificationVetting, IncidentTrackingPerformanceEvaluation, PermitToWork,
//              SiteOrientation, ActiveMonitoringAuditing) and their read-model "Get" queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/contractorsafety/{sub-component}/...
// - ContractorSafety commands/queries return PlantSafety.Application.Common.Results.Result<T>,
//   mapped to ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.ContractorSafety.Contracts.Requests;
using PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Queries;
using PlantSafety.Application.ContractorSafety.Commands;
using PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Queries;
using PlantSafety.Application.ContractorSafety.PermitToWork.Queries;
using PlantSafety.Application.ContractorSafety.PreQualificationVetting.Queries;
using PlantSafety.Application.ContractorSafety.SiteOrientation.Queries;

namespace PlantSafety.Api.ContractorSafety.Controllers.V1;

/// <summary>V1 controller for the ContractorSafety module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contractorsafety")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("ContractorSafety")]
public sealed class ContractorSafetyV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ContractorSafetyV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ContractorSafetyV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PreQualificationVetting
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Approves a PreQualificationVetting record.</summary>
    /// <response code="200">Approved.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("pre-qualification-vetting/{preQualificationVettingId:guid}/approve")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApprovePreQualificationVettingAsync(
        Guid preQualificationVettingId, [FromBody] ApprovePreQualificationVettingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ApprovePreQualificationVettingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            preQualificationVettingId, request.EmrScore, request.TrirScore, request.OshaTrainingCompliant,
            request.DeficiencyCount, request.InsuranceExpiresAtUtc, request.ValidityPeriodDays, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Begins review of a PreQualificationVetting record.</summary>
    /// <response code="200">Review begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("pre-qualification-vetting/{preQualificationVettingId:guid}/begin-review")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginReviewPreQualificationVettingAsync(
        Guid preQualificationVettingId, [FromBody] BeginReviewPreQualificationVettingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginReviewPreQualificationVettingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            preQualificationVettingId, request.ReviewedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Expires a PreQualificationVetting record.</summary>
    /// <response code="200">Expired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("pre-qualification-vetting/{preQualificationVettingId:guid}/expire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExpirePreQualificationVettingAsync(Guid preQualificationVettingId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ExpirePreQualificationVettingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            preQualificationVettingId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Rejects a PreQualificationVetting record.</summary>
    /// <response code="200">Rejected.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("pre-qualification-vetting/{preQualificationVettingId:guid}/reject")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectPreQualificationVettingAsync(
        Guid preQualificationVettingId, [FromBody] RejectPreQualificationVettingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RejectPreQualificationVettingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            preQualificationVettingId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PreQualificationVetting read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("pre-qualification-vetting")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PreQualificationVettingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPreQualificationVettingAsync([FromQuery] ContractorSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPreQualificationVettingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // IncidentTrackingPerformanceEvaluation
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Begins an investigation for an IncidentTrackingPerformanceEvaluation record.</summary>
    /// <response code="200">Investigation begun.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("incident-tracking-performance-evaluation/{incidentTrackingPerformanceEvaluationId:guid}/begin-investigation")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginInvestigationIncidentTrackingPerformanceEvaluationAsync(
        Guid incidentTrackingPerformanceEvaluationId, [FromBody] BeginInvestigationIncidentTrackingPerformanceEvaluationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new BeginInvestigationIncidentTrackingPerformanceEvaluationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            incidentTrackingPerformanceEvaluationId, request.EvaluatedByUserId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Determines eligibility for an IncidentTrackingPerformanceEvaluation record.</summary>
    /// <response code="200">Eligibility determined.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("incident-tracking-performance-evaluation/{incidentTrackingPerformanceEvaluationId:guid}/determine-eligibility")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetermineEligibilityIncidentTrackingPerformanceEvaluationAsync(
        Guid incidentTrackingPerformanceEvaluationId, [FromBody] DetermineEligibilityIncidentTrackingPerformanceEvaluationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DetermineEligibilityIncidentTrackingPerformanceEvaluationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            incidentTrackingPerformanceEvaluationId, request.PerformanceScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Documents a CAPA for an IncidentTrackingPerformanceEvaluation record.</summary>
    /// <response code="200">CAPA documented.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("incident-tracking-performance-evaluation/{incidentTrackingPerformanceEvaluationId:guid}/document-capa")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DocumentCapaIncidentTrackingPerformanceEvaluationAsync(
        Guid incidentTrackingPerformanceEvaluationId, [FromBody] DocumentCapaIncidentTrackingPerformanceEvaluationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DocumentCapaIncidentTrackingPerformanceEvaluationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            incidentTrackingPerformanceEvaluationId, request.CapaCount, request.RootCauseSummary, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns IncidentTrackingPerformanceEvaluation read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("incident-tracking-performance-evaluation")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IncidentTrackingPerformanceEvaluationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIncidentTrackingPerformanceEvaluationAsync([FromQuery] ContractorSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetIncidentTrackingPerformanceEvaluationQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PermitToWork
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Closes a PermitToWork record.</summary>
    /// <response code="200">Closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("permit-to-work/{permitToWorkId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClosePermitToWorkAsync(Guid permitToWorkId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ClosePermitToWorkCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            permitToWorkId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Expires a PermitToWork record.</summary>
    /// <response code="200">Expired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("permit-to-work/{permitToWorkId:guid}/expire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExpirePermitToWorkAsync(Guid permitToWorkId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ExpirePermitToWorkCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            permitToWorkId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Issues a PermitToWork record.</summary>
    /// <response code="200">Issued.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("permit-to-work/{permitToWorkId:guid}/issue")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssuePermitToWorkAsync(
        Guid permitToWorkId, [FromBody] IssuePermitToWorkRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new IssuePermitToWorkCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            permitToWorkId, request.AuthorizedByUserId, request.ValidityHours, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a hazard assessment for a PermitToWork record.</summary>
    /// <response code="200">Hazard assessment recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("permit-to-work/{permitToWorkId:guid}/record-hazard-assessment")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordHazardAssessmentPermitToWorkAsync(
        Guid permitToWorkId, [FromBody] RecordHazardAssessmentPermitToWorkRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordHazardAssessmentPermitToWorkCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            permitToWorkId, request.HazardCount, request.AtmosphericTestScore, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends a PermitToWork record.</summary>
    /// <response code="200">Suspended.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("permit-to-work/{permitToWorkId:guid}/suspend")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendPermitToWorkAsync(
        Guid permitToWorkId, [FromBody] SuspendPermitToWorkRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new SuspendPermitToWorkCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            permitToWorkId, request.Reason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PermitToWork read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("permit-to-work")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PermitToWorkQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermitToWorkAsync([FromQuery] ContractorSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPermitToWorkQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SiteOrientation
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Delivers orientation for a SiteOrientation record.</summary>
    /// <response code="200">Orientation delivered.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("site-orientation/{siteOrientationId:guid}/deliver-orientation")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeliverOrientationSiteOrientationAsync(
        Guid siteOrientationId, [FromBody] DeliverOrientationSiteOrientationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new DeliverOrientationSiteOrientationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            siteOrientationId, request.AttendeeCount, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Expires a SiteOrientation record.</summary>
    /// <response code="200">Expired.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("site-orientation/{siteOrientationId:guid}/expire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExpireSiteOrientationAsync(Guid siteOrientationId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ExpireSiteOrientationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            siteOrientationId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies comprehension for a SiteOrientation record.</summary>
    /// <response code="200">Comprehension verified.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("site-orientation/{siteOrientationId:guid}/verify-comprehension")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyComprehensionSiteOrientationAsync(
        Guid siteOrientationId, [FromBody] VerifyComprehensionSiteOrientationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new VerifyComprehensionSiteOrientationCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            siteOrientationId, request.ComprehensionScore, request.ValidityDays, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns SiteOrientation read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("site-orientation")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<SiteOrientationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSiteOrientationAsync([FromQuery] ContractorSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSiteOrientationQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ActiveMonitoringAuditing
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records an observation for an ActiveMonitoringAuditing record.</summary>
    /// <response code="200">Observation recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("active-monitoring-auditing/{activeMonitoringAuditingId:guid}/record-observation")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordObservationActiveMonitoringAuditingAsync(
        Guid activeMonitoringAuditingId, [FromBody] RecordObservationActiveMonitoringAuditingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordObservationActiveMonitoringAuditingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            activeMonitoringAuditingId, request.UnsafeConditionCount, request.ComplianceScore, request.FindingNotes, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves an ActiveMonitoringAuditing record.</summary>
    /// <response code="200">Resolved.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("active-monitoring-auditing/{activeMonitoringAuditingId:guid}/resolve")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveActiveMonitoringAuditingAsync(Guid activeMonitoringAuditingId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ResolveActiveMonitoringAuditingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            activeMonitoringAuditingId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a follow-up for an ActiveMonitoringAuditing record.</summary>
    /// <response code="200">Follow-up scheduled.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("active-monitoring-auditing/{activeMonitoringAuditingId:guid}/schedule-follow-up")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleFollowUpActiveMonitoringAuditingAsync(
        Guid activeMonitoringAuditingId, [FromBody] ScheduleFollowUpActiveMonitoringAuditingRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new ScheduleFollowUpActiveMonitoringAuditingCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            activeMonitoringAuditingId, request.DueAtUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ActiveMonitoringAuditing read-model data for a tenant/site within a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("active-monitoring-auditing")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ActiveMonitoringAuditingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActiveMonitoringAuditingAsync([FromQuery] ContractorSafetyQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetActiveMonitoringAuditingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
