// ============================================================================
// File: ManagementOfChangeV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.ManagementOfChange
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Compliance.ManagementOfChange module's
//              Commands and Queries over HTTP. Covers the full MOC lifecycle:
//              Create -> Approve -> Implement -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/compliance/managementofchange") does not
// collide with the pre-existing read-only category controllers under
// PlantSafety.Api/Compliance/ManagementOfChange/{Approvals,MOCWorkflows,RiskAssessments} -- those
// are routed under their own type-specific segments, confirmed by grep before finalizing this route.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - Approve uses SafetyTierPolicies.Approve; all other mutating actions use SafetyTierPolicies.Write;
//   reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.ManagementOfChange.Contracts.Requests;
using PlantSafety.Application.Compliance.ManagementOfChange.Commands;
using PlantSafety.Application.Compliance.ManagementOfChange.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.ManagementOfChange.Controllers.V1;

/// <summary>V1 controller for the Compliance.ManagementOfChange module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/managementofchange")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.ManagementOfChange")]
public sealed class ManagementOfChangeV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ManagementOfChangeV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ManagementOfChangeV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new ManagementOfChange request.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateManagementOfChangeResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateManagementOfChangeResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateManagementOfChangeRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateManagementOfChangeCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ChangeTitle: request.ChangeTitle,
            ChangeType: request.ChangeType,
            Justification: request.Justification,
            RiskLevel: request.RiskLevel,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Approves a ManagementOfChange request (Created -&gt; Active).</summary>
    [HttpPost("{managementOfChangeId:guid}/approve")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid managementOfChangeId,
        [FromBody] ApproveManagementOfChangeRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveManagementOfChangeCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ManagementOfChangeId: managementOfChangeId,
            ApprovedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records implementation of an approved change (Active -&gt; Completed).</summary>
    [HttpPost("{managementOfChangeId:guid}/implement")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImplementAsync(
        Guid managementOfChangeId,
        [FromBody] ImplementManagementOfChangeRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ImplementManagementOfChangeCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ManagementOfChangeId: managementOfChangeId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes an implemented change (Completed -&gt; Closed).</summary>
    [HttpPost("{managementOfChangeId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid managementOfChangeId,
        [FromBody] CloseManagementOfChangeRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseManagementOfChangeCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ManagementOfChangeId: managementOfChangeId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed change for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{managementOfChangeId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagementOfChangeStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid managementOfChangeId,
        [FromBody] ArchiveManagementOfChangeRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveManagementOfChangeCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ManagementOfChangeId: managementOfChangeId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single ManagementOfChange read-model record by identity, including its current ETag.</summary>
    [HttpGet("{managementOfChangeId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.ManagementOfChange.Models.ManagementOfChangeItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid managementOfChangeId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetManagementOfChangeByIdQuery(env.TenantId, managementOfChangeId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
