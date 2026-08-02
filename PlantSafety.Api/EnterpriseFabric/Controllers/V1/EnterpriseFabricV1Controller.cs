// ============================================================================
// File: EnterpriseFabricV1Controller.cs
// Project: PlantSafety.Api
// Module: EnterpriseFabric
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer EnterpriseFabric module's Commands
//              and Queries over HTTP. Covers the full lifecycle:
//              Create -> Activate -> Complete -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Route note: this controller's route ("api/v{version}/enterprise-fabric") does not collide with
// the pre-existing, unrelated Features.EnterpriseFabric.Users vertical (no controller of its own
// under this path today) or Features.EnterpriseFabric.FabricNodes.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.EnterpriseFabric.Contracts.Requests;
using PlantSafety.Application.EnterpriseFabric.Commands;
using PlantSafety.Application.EnterpriseFabric.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.EnterpriseFabric.Controllers.V1;

/// <summary>V1 controller for the EnterpriseFabric module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enterprise-fabric")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EnterpriseFabric")]
public sealed class EnterpriseFabricV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EnterpriseFabricV1Controller"/>.</summary>
    public EnterpriseFabricV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new EnterpriseFabric record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateEnterpriseFabricResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateEnterpriseFabricResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateEnterpriseFabricRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateEnterpriseFabricCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            FabricArea: request.FabricArea,
            SystemReference: request.SystemReference,
            IntegrationType: request.IntegrationType,
            OwnerUserId: request.OwnerUserId,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates an EnterpriseFabric record (Created -&gt; Active).</summary>
    [HttpPost("{enterpriseFabricId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid enterpriseFabricId,
        [FromBody] ActivateEnterpriseFabricRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateEnterpriseFabricCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnterpriseFabricId: enterpriseFabricId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an EnterpriseFabric record (Active -&gt; Completed).</summary>
    [HttpPost("{enterpriseFabricId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid enterpriseFabricId,
        [FromBody] CompleteEnterpriseFabricRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteEnterpriseFabricCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnterpriseFabricId: enterpriseFabricId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an EnterpriseFabric record (Completed -&gt; Closed).</summary>
    [HttpPost("{enterpriseFabricId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid enterpriseFabricId,
        [FromBody] CloseEnterpriseFabricRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseEnterpriseFabricCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnterpriseFabricId: enterpriseFabricId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives an EnterpriseFabric record (Closed -&gt; Archived).</summary>
    [HttpPost("{enterpriseFabricId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid enterpriseFabricId,
        [FromBody] ArchiveEnterpriseFabricRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveEnterpriseFabricCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            EnterpriseFabricId: enterpriseFabricId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single EnterpriseFabric read-model record by identity, including its current ETag.</summary>
    [HttpGet("{enterpriseFabricId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.EnterpriseFabric.Models.EnterpriseFabricItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid enterpriseFabricId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEnterpriseFabricByIdQuery(env.TenantId, enterpriseFabricId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
