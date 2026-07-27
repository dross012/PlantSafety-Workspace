// ============================================================================
// File: AuditFindingsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Auditing module's AuditFinding commands over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/auditing/findings/...
// - This is a NEW Auditing module, distinct from the pre-existing PlantSafety.Api.Compliance.Auditing
//   tree; do not merge or relocate.
//
// ============================================================================

using PlantSafety.Api.Auditing.Contracts.Requests;
using PlantSafety.Api.Context;
using PlantSafety.Application.Auditing.Commands;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Auditing.Controllers.V1;

/// <summary>V1 controller for Audit Finding commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditing/findings")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Auditing")]
public sealed class AuditFindingsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AuditFindingsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AuditFindingsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Audit Finding under an Audit Engagement.</summary>
    /// <response code="200">Audit finding created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditFindingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditFindingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditFindingResult>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateAuditFindingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateAuditFindingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            OwnerId: env.ActorId,
            AuditEngagementId: request.AuditEngagementId,
            FindingTitle: request.FindingTitle,
            Severity: request.Severity,
            CorrectiveActionRequired: request.CorrectiveActionRequired,
            AssignedToUserId: request.AssignedToUserId,
            DueDateUtc: request.DueDateUtc);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a finding reference against its parent Audit Engagement.</summary>
    /// <response code="200">Finding recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Engagement or finding not found.</response>
    [HttpPost("/api/v{version:apiVersion}/auditing/engagements/{auditEngagementId:guid}/findings/{auditFindingId:guid}/record")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordAuditEngagementFindingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordAuditEngagementFindingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RecordAuditEngagementFindingResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordAsync(
        Guid auditEngagementId,
        Guid auditFindingId,
        [FromBody] RecordAuditEngagementFindingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordAuditEngagementFindingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditEngagementId: auditEngagementId,
            AuditFindingId: auditFindingId,
            Severity: request.Severity)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Remediates an Audit Finding.</summary>
    /// <response code="200">Finding remediated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Finding not found.</response>
    [HttpPost("{auditFindingId:guid}/remediate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RemediateAuditFindingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RemediateAuditFindingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RemediateAuditFindingResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemediateAsync(
        Guid auditFindingId,
        [FromBody] RemediateAuditFindingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RemediateAuditFindingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditFindingId: auditFindingId,
            RemediationNotes: request.RemediationNotes)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Triggers a CAPA workflow from an Audit Finding.</summary>
    /// <response code="200">CAPA triggered successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Finding not found.</response>
    [HttpPost("{auditFindingId:guid}/trigger-capa")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<TriggerAuditFindingCapaResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerAuditFindingCapaResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TriggerAuditFindingCapaResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TriggerCapaAsync(
        Guid auditFindingId,
        [FromBody] TriggerAuditFindingCapaRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new TriggerAuditFindingCapaCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditFindingId: auditFindingId,
            AssignedToUserId: request.AssignedToUserId,
            DueDateUtc: request.DueDateUtc)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Verifies an Audit Finding's remediation.</summary>
    /// <response code="200">Finding verified successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Finding not found.</response>
    [HttpPost("{auditFindingId:guid}/verify")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<VerifyAuditFindingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VerifyAuditFindingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VerifyAuditFindingResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyAsync(
        Guid auditFindingId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new VerifyAuditFindingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditFindingId: auditFindingId)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
