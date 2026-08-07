// ============================================================================
// File: ShippingPapersV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.ShippingPapers
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.PHMSA.ShippingPapers
//              module's Commands and Queries over HTTP (backed by the HazmatShipment aggregate).
//              Covers the full lifecycle: Create -> GenerateShippingPaper -> MarkReady ->
//              Dispatch/Cancel, plus the side-effect-free ValidateShippingPaper action. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - ValidateShippingPaper and both GET actions use SafetyTierPolicies.Read (side-effect-free/
//   read-only); all other mutating actions use SafetyTierPolicies.Write.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.PHMSA.ShippingPapers.Contracts.Requests;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.PHMSA.ShippingPapers.Controllers.V1;

/// <summary>V1 controller for the Compliance.PHMSA.ShippingPapers module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/phmsa/shipping-papers")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.PHMSA.ShippingPapers")]
public sealed class ShippingPapersV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ShippingPapersV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ShippingPapersV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new HazmatShipment record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateHazmatShipmentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateHazmatShipmentResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateHazmatShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateHazmatShipmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            MaterialIds: request.MaterialIds,
            PackagingIds: request.PackagingIds,
            Mode: request.Mode,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Generates and attaches a DOT shipping paper to a HazmatShipment (Draft/Validated -&gt; Validated).</summary>
    [HttpPost("{hazmatShipmentId:guid}/generate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateShippingPaperAsync(
        Guid hazmatShipmentId,
        [FromBody] GenerateShippingPaperRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new GenerateShippingPaperCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatShipmentId: hazmatShipmentId,
            WasteManifestReference: request?.WasteManifestReference,
            ShipperCertificationText: request?.ShipperCertificationText,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a HazmatShipment ready for dispatch (Validated -&gt; Ready).</summary>
    [HttpPost("{hazmatShipmentId:guid}/ready")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkReadyAsync(
        Guid hazmatShipmentId,
        [FromBody] MarkShipmentReadyRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new MarkShipmentReadyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatShipmentId: hazmatShipmentId,
            ErgGuideNumber: request.ErgGuideNumber,
            EmergencyContactName: request.EmergencyContactName,
            EmergencyContactPhone: request.EmergencyContactPhone,
            ResponseGuidanceSummary: request.ResponseGuidanceSummary,
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Dispatches a HazmatShipment (Ready -&gt; Dispatched).</summary>
    [HttpPost("{hazmatShipmentId:guid}/dispatch")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DispatchAsync(
        Guid hazmatShipmentId,
        [FromBody] DispatchShipmentRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DispatchShipmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatShipmentId: hazmatShipmentId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Cancels a HazmatShipment from any non-terminal status.</summary>
    [HttpPost("{hazmatShipmentId:guid}/cancel")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatShipmentStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelAsync(
        Guid hazmatShipmentId,
        [FromBody] CancelShipmentRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CancelShipmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatShipmentId: hazmatShipmentId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Re-evaluates the shipping paper for an existing HazmatShipment and compares it to what is currently persisted.</summary>
    [HttpPost("{hazmatShipmentId:guid}/validate")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ValidateShippingPaperResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidateShippingPaperResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidateShippingPaperAsync(
        Guid hazmatShipmentId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ValidateShippingPaperCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatShipmentId: hazmatShipmentId,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single HazmatShipment read-model record by identity, including its current ETag.</summary>
    [HttpGet("{hazmatShipmentId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Models.HazmatShipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid hazmatShipmentId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazmatShipmentQuery(env.TenantId, hazmatShipmentId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazmatShipment read-model data for a tenant/site, optionally filtered by Status and/or Mode.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListHazmatShipmentsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListHazmatShipmentsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] HazmatShipmentListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListHazmatShipmentsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Status: request.Status,
            Mode: request.Mode);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
