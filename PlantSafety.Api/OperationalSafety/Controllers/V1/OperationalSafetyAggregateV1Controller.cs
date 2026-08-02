// ============================================================================
// File: OperationalSafetyAggregateV1Controller.cs
// Project: PlantSafety.Api
// Module: OperationalSafety
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer OperationalSafety aggregate-root
//              module's Commands and Queries over HTTP. Covers the full lifecycle:
//              Create -> Activate -> Complete -> Close -> Archive. Named distinctly
//              ("OperationalSafetyAggregateV1Controller", not "OperationalSafetyV1Controller")
//              and routed under "operational-safety/records" to avoid any route collision with
//              the pre-existing, unrelated, LIVE OperationalSafety.Alerts/Checklists/
//              PPEManagement/RiskManagement/Schedules controllers. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.OperationalSafety.Contracts.Requests;
using PlantSafety.Application.OperationalSafety.Commands;
using PlantSafety.Application.OperationalSafety.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.OperationalSafety.Controllers.V1;

/// <summary>V1 controller for the OperationalSafety aggregate-root module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational-safety/records")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("OperationalSafety")]
public sealed class OperationalSafetyAggregateV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="OperationalSafetyAggregateV1Controller"/>.</summary>
    public OperationalSafetyAggregateV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new OperationalSafety record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateOperationalSafetyResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateOperationalSafetyResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOperationalSafetyRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateOperationalSafetyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            SafetyDomain: request.SafetyDomain,
            RiskLevel: request.RiskLevel,
            OwnerUserId: request.OwnerUserId,
            ReviewDueAtUtc: request.ReviewDueAtUtc,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates an OperationalSafety record (Created -&gt; Active).</summary>
    [HttpPost("{operationalSafetyId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid operationalSafetyId,
        [FromBody] ActivateOperationalSafetyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateOperationalSafetyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            OperationalSafetyId: operationalSafetyId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an OperationalSafety record (Active -&gt; Completed).</summary>
    [HttpPost("{operationalSafetyId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid operationalSafetyId,
        [FromBody] CompleteOperationalSafetyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteOperationalSafetyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            OperationalSafetyId: operationalSafetyId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an OperationalSafety record (Completed -&gt; Closed).</summary>
    [HttpPost("{operationalSafetyId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid operationalSafetyId,
        [FromBody] CloseOperationalSafetyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseOperationalSafetyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            OperationalSafetyId: operationalSafetyId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed OperationalSafety record for long-term retention (Closed -&gt; Archived).</summary>
    [HttpPost("{operationalSafetyId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OperationalSafetyStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid operationalSafetyId,
        [FromBody] ArchiveOperationalSafetyRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveOperationalSafetyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            OperationalSafetyId: operationalSafetyId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single OperationalSafety read-model record by identity, including its current ETag.</summary>
    [HttpGet("{operationalSafetyId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.OperationalSafety.Models.OperationalSafetyItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid operationalSafetyId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetOperationalSafetyByIdQuery(env.TenantId, operationalSafetyId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
