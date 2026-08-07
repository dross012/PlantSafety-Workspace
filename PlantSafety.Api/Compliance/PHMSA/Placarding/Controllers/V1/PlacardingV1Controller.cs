// ============================================================================
// File: PlacardingV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Placarding
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.PHMSA.Placarding module's
//              Commands and Queries over HTTP. Covers the full HazmatContainer lifecycle:
//              Create -> ApplyMarkingsAndLabels -> ApplyPlacards -> Retire, plus the
//              side-effect-free CalculatePlacards and ValidatePlacardingForShipment actions.
//              Mirrors PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - CalculatePlacards, ValidatePlacardingForShipment, and both GET actions use
//   SafetyTierPolicies.Read (side-effect-free/read-only); all other mutating actions use
//   SafetyTierPolicies.Write.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.PHMSA.Placarding.Contracts.Requests;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.PHMSA.Placarding.Controllers.V1;

/// <summary>V1 controller for the Compliance.PHMSA.Placarding module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/phmsa/placarding")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.PHMSA.Placarding")]
public sealed class PlacardingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PlacardingV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PlacardingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new HazmatContainer record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateHazmatContainerResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateHazmatContainerResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateHazmatContainerRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateHazmatContainerCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ContainerType: request.ContainerType,
            MaterialId: request.MaterialId,
            PackagingSpecId: request.PackagingSpecId,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Applies text markings and hazard labels to a HazmatContainer (Draft -&gt; MarkedAndLabeled).</summary>
    [HttpPost("{hazmatContainerId:guid}/mark")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatContainerStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatContainerStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApplyMarkingsAndLabelsAsync(
        Guid hazmatContainerId,
        [FromBody] ApplyMarkingsAndLabelsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApplyMarkingsAndLabelsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatContainerId: hazmatContainerId,
            Markings: request.Markings,
            Labels: request.Labels,
            OrientationArrowsRequired: request.OrientationArrowsRequired,
            CompatibilityGroup: request.CompatibilityGroup,
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Computes and records the required placard set for a HazmatContainer (MarkedAndLabeled -&gt; PlacardingCalculated).</summary>
    [HttpPost("{hazmatContainerId:guid}/placards")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatContainerStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatContainerStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApplyPlacardsAsync(
        Guid hazmatContainerId,
        [FromBody] ApplyPlacardsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApplyPlacardsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatContainerId: hazmatContainerId,
            Materials: request.Materials,
            IsBulkPackaging: request.IsBulkPackaging,
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a HazmatContainer record from active use.</summary>
    [HttpPost("{hazmatContainerId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatContainerStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatContainerStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetireAsync(
        Guid hazmatContainerId,
        [FromBody] RetireHazmatContainerRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetireHazmatContainerCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatContainerId: hazmatContainerId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Computes the required DOT transport placard set for a proposed load, with no persistence.</summary>
    [HttpPost("calculate")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<CalculatePlacardsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CalculatePlacardsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CalculatePlacardsAsync(
        [FromBody] CalculatePlacardsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CalculatePlacardsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            Materials: request.Materials,
            IsBulkPackaging: request.IsBulkPackaging,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Re-evaluates placarding for an existing HazmatShipment and compares it to what is currently persisted.</summary>
    [HttpPost("validate/{shipmentId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ValidatePlacardingForShipmentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidatePlacardingForShipmentResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidatePlacardingForShipmentAsync(
        Guid shipmentId,
        [FromBody] ValidatePlacardingForShipmentRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ValidatePlacardingForShipmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ShipmentId: shipmentId,
            IsBulkPackaging: request?.IsBulkPackaging ?? false,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single HazmatContainer read-model record by identity, including its current ETag.</summary>
    [HttpGet("{hazmatContainerId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.Placarding.Models.HazmatContainerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid hazmatContainerId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazmatContainerQuery(env.TenantId, hazmatContainerId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazmatContainer read-model data for a tenant/site, optionally filtered by MaterialId.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListHazmatContainersResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListHazmatContainersResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] HazmatContainerListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListHazmatContainersQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            MaterialId: request.MaterialId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
