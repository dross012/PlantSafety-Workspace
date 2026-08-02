// ============================================================================
// File: AuditingV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.Auditing
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Compliance.Auditing module's Commands
//              and Queries over HTTP. Covers the full audit lifecycle:
//              Create -> Start -> RecordFindings -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/compliance/auditing") does not collide
// with the pre-existing read-only category controllers under
// PlantSafety.Api/Compliance/Auditing/<Category>/Controllers (AuditFindings/CAPATriggers/
// ComplianceAudit/InternalSafetyAudit) -- those are routed via their own nested segments,
// confirmed by grep before finalizing this route.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.Auditing.Contracts.Requests;
using PlantSafety.Application.Compliance.Auditing.Commands;
using PlantSafety.Application.Compliance.Auditing.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.Auditing.Controllers.V1;

/// <summary>V1 controller for the Compliance.Auditing module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/auditing")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.Auditing")]
public sealed class AuditingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AuditingV1Controller"/>.</summary>
    public AuditingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Auditing record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateAuditingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateAuditingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            AuditTitle: request.AuditTitle,
            AuditType: request.AuditType,
            Scope: request.Scope,
            AuditorId: request.AuditorId,
            ScheduledAtUtc: request.ScheduledAtUtc,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts an audit program/execution (Created -&gt; Active).</summary>
    [HttpPost("{auditingId:guid}/start")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(
        Guid auditingId,
        [FromBody] StartAuditingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartAuditingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AuditingId: auditingId,
            StartedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an audit's findings and compliance score, completing the audit (Active -&gt; Completed).</summary>
    [HttpPost("{auditingId:guid}/record-findings")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordFindingsAsync(
        Guid auditingId,
        [FromBody] RecordAuditingFindingsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordAuditingFindingsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AuditingId: auditingId,
            CompletedByUserId: env.ActorId,
            FindingsCount: request.FindingsCount,
            Score: request.Score,
            ComplianceRating: request.ComplianceRating)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed audit (Completed -&gt; Closed).</summary>
    [HttpPost("{auditingId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid auditingId,
        [FromBody] CloseAuditingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseAuditingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AuditingId: auditingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed audit for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{auditingId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid auditingId,
        [FromBody] ArchiveAuditingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveAuditingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            AuditingId: auditingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Auditing read-model record by identity, including its current ETag.</summary>
    [HttpGet("{auditingId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid auditingId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAuditingByIdQuery(env.TenantId, auditingId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
