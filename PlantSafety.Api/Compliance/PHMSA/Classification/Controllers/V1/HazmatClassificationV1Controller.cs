// ============================================================================
// File: HazmatClassificationV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Classification
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.PHMSA.Classification
//              module's Commands and Queries over HTTP. Covers the hazmat classification
//              lifecycle: Classify -> Supersede/Retire. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.PHMSA.Classification.Contracts.Requests;
using PlantSafety.Application.Compliance.PHMSA.Classification.Commands;
using PlantSafety.Application.Compliance.PHMSA.Classification.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.PHMSA.Classification.Controllers.V1;

/// <summary>V1 controller for the Compliance.PHMSA.Classification module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/phmsa/classification")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.PHMSA.Classification")]
public sealed class HazmatClassificationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="HazmatClassificationV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public HazmatClassificationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Classifies a candidate hazardous material under 49 CFR/PHMSA.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ClassifyMaterialResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClassifyMaterialResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ClassifyAsync(
        [FromBody] ClassifyMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ClassifyMaterialCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            MaterialName: request.MaterialName,
            CasNumber: request.CasNumber,
            WasteProfileCode: request.WasteProfileCode,
            SdsHazardStatement: request.SdsHazardStatement,
            ClassifiedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a HazmatMaterial classification superseded by a newer classification record.</summary>
    [HttpPost("{hazmatMaterialId:guid}/supersede")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatMaterialStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatMaterialStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SupersedeAsync(
        Guid hazmatMaterialId,
        [FromBody] SupersedeHazmatMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SupersedeHazmatMaterialCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatMaterialId: hazmatMaterialId,
            SupersededByMaterialId: request.SupersededByMaterialId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a HazmatMaterial classification record.</summary>
    [HttpPost("{hazmatMaterialId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazmatMaterialStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazmatMaterialStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetireAsync(
        Guid hazmatMaterialId,
        [FromBody] RetireHazmatMaterialRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetireHazmatMaterialCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            HazmatMaterialId: hazmatMaterialId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single HazmatMaterial read-model record by identity, including its current ETag.</summary>
    [HttpGet("{hazmatMaterialId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Compliance.PHMSA.Classification.Models.HazmatMaterialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid hazmatMaterialId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazmatMaterialQuery(env.TenantId, hazmatMaterialId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazmatMaterial read-model data for a tenant/site, optionally filtered by UN number.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListClassifiedMaterialsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ListClassifiedMaterialsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListClassifiedMaterialsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListClassifiedMaterialsQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            UNNumber: request.UNNumber);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
