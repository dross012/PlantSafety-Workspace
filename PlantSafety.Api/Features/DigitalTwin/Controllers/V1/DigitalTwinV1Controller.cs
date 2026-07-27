// ============================================================================
// File: DigitalTwinV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing Digital Twin Commands and Queries from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/digital-twin/twins (matches the route already coded
//   into PlantSafety.UI's DigitalTwinApiClient).
// - Command/Query envelope fields are populated via RequestEnvelopeFactory.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.DigitalTwin.Contracts.Requests;
using PlantSafety.Api.Features.DigitalTwin.Contracts.Responses;
using PlantSafety.Application.Features.DigitalTwin.Commands.RegisterTwin;
using PlantSafety.Application.Features.DigitalTwin.Models;
using PlantSafety.Application.Features.DigitalTwin.Queries.GetTwin;
using PlantSafety.Application.Features.DigitalTwin.Queries.ListTwins;

namespace PlantSafety.Api.Features.DigitalTwin.Controllers.V1;

/// <summary>V1 controller for Digital Twin command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/digital-twin/twins")]
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

    /// <summary>Registers a new Digital Twin in Provisioning status.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RegisterDigitalTwinResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterDigitalTwinRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RegisterDigitalTwinCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            AssetName = request.AssetName,
            AssetType = request.AssetType,
        };

        var twinId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<RegisterDigitalTwinResponse>.Ok(new RegisterDigitalTwinResponse(twinId), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single Digital Twin asset by identifier.</summary>
    [HttpGet("{twinId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinAssetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid twinId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDigitalTwinByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            TwinId = twinId,
        };

        var twin = await _sender.Send(query, cancellationToken);

        if (twin is null)
            return NotFound(ApiResponse<DigitalTwinAssetResponse>.Fail("Digital twin not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<DigitalTwinAssetResponse>.Ok(ToResponse(twin), env.CorrelationId.ToString()));
    }

    /// <summary>Returns the Digital Twin asset registry for the tenant.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<DigitalTwinListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListDigitalTwinsQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            SiteId = siteId,
        };

        var items = await _sender.Send(query, cancellationToken);

        var response = new DigitalTwinListResponse(env.TenantId, siteId, DateTime.UtcNow, items.Select(ToResponse).ToList());

        return Ok(ApiResponse<DigitalTwinListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static DigitalTwinAssetResponse ToResponse(DigitalTwinAssetDto dto) => new(
        dto.TwinId,
        dto.TenantId,
        dto.SiteId,
        dto.AssetName,
        dto.AssetType,
        dto.Status,
        dto.LastSyncAtUtc,
        dto.TelemetryHealthScore);
}
