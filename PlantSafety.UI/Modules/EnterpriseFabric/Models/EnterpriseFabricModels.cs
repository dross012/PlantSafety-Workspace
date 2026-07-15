// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Enterprise Fabric module. No
//   PlantSafety.Api read-model (GET) endpoint exists yet for this module
//   today — PlantSafety.Domain/EnterpriseFabric is a thin
//   Id/CreatedAtUtc/CreatedBy scaffold aggregate, and the only wired
//   Api/Features/EnterpriseFabric surface is a write-only CreateUserCommand
//   unrelated to fabric topology. This shape (integration fabric node
//   registry entries) is the best-inferable read-model DTO so the UI is
//   ready to bind the instant a real endpoint ships. Until then,
//   EnterpriseFabricApiClient calls gracefully fault to the page's Error
//   state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.EnterpriseFabric.Models;

/// <summary>Represents a single integration fabric node (connector/gateway/broker/adapter).</summary>
public sealed class FabricNodeDto
{
    /// <summary>Gets the unique identifier of the fabric node.</summary>
    public Guid NodeId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the display name of the fabric node.</summary>
    public string NodeName { get; init; } = string.Empty;

    /// <summary>Gets the node type (e.g., Connector, Gateway, Broker, Adapter).</summary>
    public string NodeType { get; init; } = string.Empty;

    /// <summary>Gets the current connectivity status (e.g., Connected/Degraded/Offline).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the node's last heartbeat.</summary>
    public DateTime LastHeartbeatAtUtc { get; init; }

    /// <summary>Gets the deployment region for this node.</summary>
    public string Region { get; init; } = string.Empty;
}

/// <summary>Envelope for the (not-yet-wired) GET v1/enterprise-fabric/nodes endpoint.</summary>
public sealed class EnterpriseFabricResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the fabric node items.</summary>
    public IReadOnlyList<FabricNodeDto> Items { get; init; } = [];
}
