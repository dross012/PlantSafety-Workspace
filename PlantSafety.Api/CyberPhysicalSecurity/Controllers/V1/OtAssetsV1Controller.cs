// ============================================================================
// File: OtAssetsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-04
// Description: V1 MVC controller exposing the OT Asset Inventory subdomain of the
//              CyberPhysicalSecurity module. Backs the OT Asset Security Status dashboard.
// ============================================================================
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.CyberPhysicalSecurity.Contracts.Requests;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Queries;

namespace PlantSafety.Api.CyberPhysicalSecurity.Controllers.V1;

/// <summary>V1 controller for the OT Asset Inventory subdomain.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cyberphysicalsecurity/ot-assets")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("CyberPhysicalSecurity.OtAssets")]
public sealed class OtAssetsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="OtAssetsV1Controller"/>.</summary>
    public OtAssetsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Registers a new OT asset in the inventory.</summary>
    /// <response code="200">OT asset registered successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterOtAssetRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RegisterOtAssetCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.TenantId, request.SiteId, request.AssetTag, request.AssetName, request.AssetType,
            request.Vendor ?? string.Empty, request.Model ?? string.Empty, request.FirmwareVersion ?? string.Empty,
            request.NetworkZone ?? string.Empty, request.Criticality ?? "Medium", env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single OT asset's current inventory record.</summary>
    /// <response code="200">OT asset retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">No OT asset exists with the given identity for this tenant.</response>
    [HttpGet("{otAssetId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid otAssetId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetOtAssetByIdQuery(env.TenantId, otAssetId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
