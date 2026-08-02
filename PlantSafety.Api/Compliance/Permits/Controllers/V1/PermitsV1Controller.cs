// ============================================================================
// File: PermitsV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.Permits
// Created: 2026-07-30
// Description: V1 controller exposing the Application-layer Compliance.Permits module's Commands
//              and Queries over HTTP. Covers the full Permit-to-Work lifecycle:
//              Create -> Approve/Reject -> Complete/Expire -> Close -> Archive. Mirrors
//              PlantSafety.Api.PermitToWork.Controllers.V1.PermitToWorkV1Controller.
//
// Route note: this controller's route ("api/v{version}/compliance/permits") does not collide
// with the 13 pre-existing read-only "permit type" category controllers under
// PlantSafety.Api/Compliance/Permits/<Type>/Controllers -- those are routed via each type's own
// *AuthorizationPolicies-adjacent attribute routing nested under type-specific segments (e.g.
// .../compliance/permits/hotwork via HotWorkV1Controller), confirmed by grep before finalizing
// this route. It also does not collide with the unrelated, out-of-scope
// PlantSafety.Api.Features.Compliance.Permits.Controllers.V1.PermitsV1Controller, which is routed
// under "api/v{version}/features/compliance/permits" (the separate "Features" tree against the
// singular [Compliance].[Permit] table).
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - Approve/Reject use SafetyTierPolicies.Approve (mirrors PermitToWorkV1Controller's Approve/
//   Reject policy choice); all other mutating actions use SafetyTierPolicies.Write; reads use
//   SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.Permits.Contracts.Requests;
using PlantSafety.Application.Compliance.Permits.Commands;
using PlantSafety.Application.Compliance.Permits.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.Permits.Controllers.V1;

/// <summary>V1 controller for the Compliance.Permits module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/permits")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.Permits")]
public sealed class PermitsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PermitsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PermitsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new Permits request.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreatePermitsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreatePermitsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePermitsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreatePermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            PermitType: request.PermitType,
            PermitNumber: request.PermitNumber,
            WorkDescription: request.WorkDescription,
            Location: request.Location,
            HazardsIdentified: request.HazardsIdentified,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Approves and issues a Permits request (Created -&gt; Active).</summary>
    [HttpPost("{permitsId:guid}/approve")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid permitsId,
        [FromBody] ApprovePermitsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApprovePermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitsId: permitsId,
            ApprovedByUserId: env.ActorId,
            ValidityHours: request?.ValidityHours)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Declines a Permits request (Created -&gt; Rejected).</summary>
    [HttpPost("{permitsId:guid}/reject")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RejectAsync(
        Guid permitsId,
        [FromBody] RejectPermitsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RejectPermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitsId: permitsId,
            RejectedByUserId: env.ActorId,
            Reason: request.Reason)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks the permitted work completed (Active -&gt; Completed).</summary>
    [HttpPost("{permitsId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid permitsId,
        [FromBody] CompletePermitsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompletePermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitsId: permitsId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Expires an issued permit that passed its validity window (Active -&gt; Expired).</summary>
    [HttpPost("{permitsId:guid}/expire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExpireAsync(
        Guid permitsId,
        [FromBody] ExpirePermitsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ExpirePermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitsId: permitsId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed permit (Completed -&gt; Closed).</summary>
    [HttpPost("{permitsId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid permitsId,
        [FromBody] ClosePermitsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ClosePermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitsId: permitsId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed permit for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{permitsId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid permitsId,
        [FromBody] ArchivePermitsRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchivePermitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PermitsId: permitsId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Permits read-model record by identity, including its current ETag.</summary>
    [HttpGet("{permitsId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid permitsId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitsByIdQuery(env.TenantId, permitsId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Permits read-model data for a tenant/site within a date range.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PermitsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PermitsQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermitsAsync(
        [FromQuery] PermitsListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
