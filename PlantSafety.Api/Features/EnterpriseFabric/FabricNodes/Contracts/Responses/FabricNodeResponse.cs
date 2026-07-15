// ============================================================================
// File: FabricNodeResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTOs for the Enterprise Fabric module.
// ============================================================================

namespace PlantSafety.Api.Features.EnterpriseFabric.FabricNodes.Contracts.Responses;

/// <summary>Response payload for a single integration fabric node.</summary>
public sealed record FabricNodeResponse(
    Guid     NodeId,
    Guid     TenantId,
    Guid?    SiteId,
    string   NodeName,
    string   NodeType,
    string   Status,
    DateTime LastHeartbeatAtUtc,
    string   Region);

/// <summary>Envelope response payload for the fabric node listing.</summary>
public sealed record EnterpriseFabricListResponse(
    Guid                              TenantId,
    Guid?                             SiteId,
    DateTime                          GeneratedAt,
    IReadOnlyList<FabricNodeResponse> Items);

/// <summary>Response payload returned after registering a fabric node.</summary>
/// <param name="NodeId">Identifier of the newly registered node.</param>
public sealed record RegisterFabricNodeResponse(Guid NodeId);
