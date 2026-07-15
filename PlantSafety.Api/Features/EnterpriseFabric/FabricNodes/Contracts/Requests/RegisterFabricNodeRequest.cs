// ============================================================================
// File: RegisterFabricNodeRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for POST v1/enterprise-fabric/nodes,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.EnterpriseFabric.FabricNodes.Contracts.Requests;

/// <summary>Request body for registering a new integration fabric node.</summary>
public sealed record RegisterFabricNodeRequest(
    Guid?  SiteId,
    string NodeName,
    string NodeType,
    string Region);
