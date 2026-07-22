// ============================================================================
// File: PermitToWorkV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Application-layer PermitToWork module's Commands and
//              Queries over HTTP. Covers the full PTW lifecycle: Request -> AssessHazards ->
//              ImplementControls -> Approve -> Issue -> RecordClosureSignature -> Archive, plus
//              Reject, and the PermitTemplate Publish/Deprecate lifecycle.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/permittowork/...
// - PermitWorkflow lifecycle commands hang directly off the module root (e.g.
//   POST /api/v1/permittowork/{permitWorkflowId}/assess-hazards) per the live-verified
//   end-to-end workflow. PermitTemplate commands live under /templates/{id}/... to
//   disambiguate the aggregate.
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
using PlantSafety.Api.PermitToWork.Contracts.Requests;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PermitToWork.Commands;
using PlantSafety.Application.PermitToWork.PermitExpiration.Queries;
using PlantSafety.Application.PermitToWork.PermitScheduling.Queries;
using PlantSafety.Application.PermitToWork.PermitTemplate.Queries;
using PlantSafety.Application.PermitToWork.PermitWorkflow.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.PermitToWork.Controllers.V1;

/// <summary>V1 controller for the PermitToWork module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/permittowork")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("PermitToWork")]
public sealed class PermitToWorkV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PermitToWorkV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PermitToWorkV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ── PermitWorkflow lifecycle ─────────────────────────────────────────────

    /// <summary>Submits a new Permit-to-Work request (PTW Workflow Step 1 - Permit Request).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestAsync(
        [FromBody] RequestPermitWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RequestPermitWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            PermitNumber: request.PermitNumber,
            PermitTypeCode: request.PermitTypeCode,
            RequesterUserId: env.ActorId,
            WorkDescription: request.WorkDescription);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the safety officer's hazard assessment and required controls (PTW Workflow Step 2).</summary>
    [HttpPost("{permitWorkflowId:guid}/assess-hazards")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssessHazardsAsync(
        Guid permitWorkflowId,
        [FromBody] AssessPermitHazardsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssessPermitHazardsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            RequiredControls: request.RequiredControls,
            RiskScore: request.RiskScore,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that required safety controls have been implemented (PTW Workflow Step 3).</summary>
    [HttpPost("{permitWorkflowId:guid}/implement-controls")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImplementControlsAsync(
        Guid permitWorkflowId,
        [FromBody] ImplementPermitControlsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ImplementPermitControlsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            ControlsConfirmedBy: request.ControlsConfirmedBy,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records supervisor/safety-officer approval of a PermitWorkflow (PTW Workflow Step 4 - Authorization).</summary>
    [HttpPost("{permitWorkflowId:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid permitWorkflowId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApprovePermitWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            ApprovedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Issues an approved PermitWorkflow, making it active (PTW Workflow Step 4/5).</summary>
    [HttpPost("{permitWorkflowId:guid}/issue")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IssueAsync(
        Guid permitWorkflowId,
        [FromBody] IssuePermitWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new IssuePermitWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            IssuerUserId: env.ActorId,
            ValidityHours: request.ValidityHours);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the post-work review and closure signature for a permit, closing it (PTW Workflow Step 6 - Close-Out).</summary>
    [HttpPost("{permitWorkflowId:guid}/record-closure-signature")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordClosureSignatureAsync(
        Guid permitWorkflowId,
        [FromBody] RecordPermitClosureSignatureRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordPermitClosureSignatureCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            ClosureSignedByUserId: env.ActorId,
            ClosureNotes: request.ClosureNotes);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed PermitWorkflow for compliance retention (PTW Workflow Step 7 - Review &amp; Audit).</summary>
    [HttpPost("{permitWorkflowId:guid}/archive")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid permitWorkflowId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchivePermitWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Declines a PermitWorkflow request, recording the reason for rejection.</summary>
    [HttpPost("{permitWorkflowId:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RejectAsync(
        Guid permitWorkflowId,
        [FromBody] RejectPermitWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RejectPermitWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitWorkflowId: permitWorkflowId,
            RejectedByUserId: env.ActorId,
            Reason: request.Reason);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PermitWorkflow read-model data for a tenant/site within a date range.</summary>
    [HttpGet("workflow")]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitWorkflowQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermitWorkflowAsync(
        [FromQuery] PermitToWorkListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitWorkflowQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PermitTemplate lifecycle ─────────────────────────────────────────────

    /// <summary>Publishes a draft PermitTemplate, making it active for permit requests.</summary>
    [HttpPost("templates/{permitTemplateId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<PermitTemplateActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitTemplateActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishPermitTemplateAsync(
        Guid permitTemplateId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishPermitTemplateCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitTemplateId: permitTemplateId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Deprecates an active PermitTemplate so no new permits may be requested against it.</summary>
    [HttpPost("templates/{permitTemplateId:guid}/deprecate")]
    [ProducesResponseType(typeof(ApiResponse<PermitTemplateActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitTemplateActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeprecatePermitTemplateAsync(
        Guid permitTemplateId,
        [FromBody] DeprecatePermitTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeprecatePermitTemplateCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitTemplateId: permitTemplateId,
            PerformedByUserId: env.ActorId,
            Reason: request.Reason);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PermitTemplate read-model data for a tenant/site within a date range.</summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<PermitTemplateQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitTemplateQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermitTemplateAsync(
        [FromQuery] PermitToWorkListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitTemplateQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PermitScheduling ─────────────────────────────────────────────────────

    /// <summary>Returns PermitScheduling read-model data for a tenant/site within a date range.</summary>
    [HttpGet("scheduling")]
    [ProducesResponseType(typeof(ApiResponse<PermitSchedulingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitSchedulingQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermitSchedulingAsync(
        [FromQuery] PermitToWorkListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitSchedulingQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PermitExpiration ─────────────────────────────────────────────────────

    /// <summary>Returns PermitExpiration read-model data for a tenant/site within a date range.</summary>
    [HttpGet("expiration")]
    [ProducesResponseType(typeof(ApiResponse<PermitExpirationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitExpirationQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermitExpirationAsync(
        [FromQuery] PermitToWorkListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitExpirationQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Bulk ─────────────────────────────────────────────────────────────────

    /// <summary>Applies a single operation uniformly across multiple PermitToWork resources.</summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdatePermitToWorkResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdatePermitToWorkResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateAsync(
        [FromBody] BulkUpdatePermitToWorkRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new BulkUpdatePermitToWorkCommand(
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
