// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for an integration fabric node registry entry.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Models.FabricNodes;

/// <summary>
/// Read-side projection of an integration fabric node (connector/gateway/broker/adapter)
/// used to integrate PlantSafety with external systems (HRIS/ERP/CMMS/LMS/MES/Supplier).
/// </summary>
public sealed record FabricNodeDto
{
    /// <summary>Unique identifier of the fabric node (UUIDv7).</summary>
    public required Guid NodeId { get; init; }

    /// <summary>Tenant owning this node.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this node is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the fabric node.</summary>
    public required string NodeName { get; init; }

    /// <summary>Node type (e.g., Connector, Gateway, Broker, Adapter).</summary>
    public required string NodeType { get; init; }

    /// <summary>Current connectivity status (e.g., Connected, Degraded, Offline).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of the node's last heartbeat.</summary>
    public required DateTime LastHeartbeatAtUtc { get; init; }

    /// <summary>Deployment region for this node.</summary>
    public required string Region { get; init; }
}
