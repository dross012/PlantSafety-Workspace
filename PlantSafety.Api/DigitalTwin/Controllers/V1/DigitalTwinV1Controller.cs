// ============================================================================
// File: DigitalTwinV1Controller.cs
// Project: PlantSafety.Api
// Module: DigitalTwin
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer DigitalTwin module's Commands and
//              Queries over HTTP. Covers the full lifecycle:
//              Create -> Activate -> RefreshSync -> Complete -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Route note: this controller's route ("api/v{version}/digital-twin") is distinct from, and does
// not collide with, the pre-existing "Twin" Features vertical which is NOT routed under this path.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.DigitalTwin.Contracts.Requests;
using PlantSafety.Application.DigitalTwin.Commands;
using PlantSafety.Application.DigitalTwin.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.DigitalTwin.Controllers.V1;

/// <summary>V1 controller for the DigitalTwin module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/digital-twin")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("DigitalTwin")]
public sealed class DigitalTwinV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DigitalTwinV1Controller"/>.</summary>
    public DigitalTwinV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Registers a new DigitalTwin model registry record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateDigitalTwinResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateDigitalTwinResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDigitalTwinRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateDigitalTwinCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            TwinName: request.TwinName,
            AssetReferenceId: request.AssetReferenceId,
            ModelType: request.ModelType,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a DigitalTwin record (Created -&gt; Active).</summary>
    [HttpPost("{digitalTwinId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid digitalTwinId,
        [FromBody] ActivateDigitalTwinRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateDigitalTwinCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DigitalTwinId: digitalTwinId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Refreshes a DigitalTwin's last-synchronized timestamp (same-status action while Active).</summary>
    [HttpPost("{digitalTwinId:guid}/refresh-sync")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshSyncAsync(
        Guid digitalTwinId,
        [FromBody] RefreshSyncDigitalTwinRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RefreshSyncDigitalTwinCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DigitalTwinId: digitalTwinId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a DigitalTwin record (Active -&gt; Completed).</summary>
    [HttpPost("{digitalTwinId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid digitalTwinId,
        [FromBody] CompleteDigitalTwinRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteDigitalTwinCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DigitalTwinId: digitalTwinId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a DigitalTwin record (Completed -&gt; Closed).</summary>
    [HttpPost("{digitalTwinId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid digitalTwinId,
        [FromBody] CloseDigitalTwinRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseDigitalTwinCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DigitalTwinId: digitalTwinId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a DigitalTwin record (Closed -&gt; Archived).</summary>
    [HttpPost("{digitalTwinId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid digitalTwinId,
        [FromBody] ArchiveDigitalTwinRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveDigitalTwinCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DigitalTwinId: digitalTwinId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single DigitalTwin read-model record by identity, including its current ETag.</summary>
    [HttpGet("{digitalTwinId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.DigitalTwin.Models.DigitalTwinItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid digitalTwinId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDigitalTwinByIdQuery(env.TenantId, digitalTwinId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
