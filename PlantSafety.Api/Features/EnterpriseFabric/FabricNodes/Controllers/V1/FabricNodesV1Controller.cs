// ============================================================================
// File: FabricNodesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing Enterprise Fabric node Commands and Queries from
//              the Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/enterprise-fabric/nodes (matches the route already
//   coded into PlantSafety.UI's EnterpriseFabricApiClient).
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.EnterpriseFabric.FabricNodes.Contracts.Requests;
using PlantSafety.Api.Features.EnterpriseFabric.FabricNodes.Contracts.Responses;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.FabricNodes.RegisterNode;
using PlantSafety.Application.Features.EnterpriseFabric.Models.FabricNodes;
using PlantSafety.Application.Features.EnterpriseFabric.Queries.FabricNodes.GetNode;
using PlantSafety.Application.Features.EnterpriseFabric.Queries.FabricNodes.ListNodes;

namespace PlantSafety.Api.Features.EnterpriseFabric.FabricNodes.Controllers.V1;

/// <summary>V1 controller for Enterprise Fabric node command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enterprise-fabric/nodes")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("EnterpriseFabric")]
public sealed class FabricNodesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="FabricNodesV1Controller"/>.</summary>
    public FabricNodesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Registers a new integration fabric node.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RegisterFabricNodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterFabricNodeRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RegisterFabricNodeCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            NodeName = request.NodeName,
            NodeType = request.NodeType,
            Region = request.Region,
        };

        var nodeId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<RegisterFabricNodeResponse>.Ok(
            new RegisterFabricNodeResponse(nodeId), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single fabric node by identifier.</summary>
    [HttpGet("{nodeId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<FabricNodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid nodeId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetFabricNodeByIdQuery
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
            NodeId = nodeId,
        };

        var node = await _sender.Send(query, cancellationToken);

        if (node is null)
            return NotFound(ApiResponse<FabricNodeResponse>.Fail("Fabric node not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<FabricNodeResponse>.Ok(ToResponse(node), env.CorrelationId.ToString()));
    }

    /// <summary>Returns the integration fabric node registry for the tenant.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<EnterpriseFabricListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListFabricNodesQuery
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

        var response = new EnterpriseFabricListResponse(env.TenantId, siteId, DateTime.UtcNow, items.Select(ToResponse).ToList());

        return Ok(ApiResponse<EnterpriseFabricListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static FabricNodeResponse ToResponse(FabricNodeDto dto) => new(
        dto.NodeId,
        dto.TenantId,
        dto.SiteId,
        dto.NodeName,
        dto.NodeType,
        dto.Status,
        dto.LastHeartbeatAtUtc,
        dto.Region);
}
