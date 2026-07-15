// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Repository interface for the integration fabric node read/write model.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.FabricNodes;

/// <summary>Repository for integration fabric node read/write operations.</summary>
public interface IFabricNodeRepository
{
    /// <summary>Registers a new fabric node; stages a FabricNodeRegisteredEvent in the outbox.</summary>
    Task CreateAsync(
        Guid nodeId,
        Guid tenantId,
        Guid? siteId,
        string nodeName,
        string nodeType,
        string region,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<FabricNodeReadModel?> GetByIdAsync(Guid tenantId, Guid nodeId, CancellationToken cancellationToken);

    /// <summary>Returns the fabric node registry for a tenant, optionally scoped to a site.</summary>
    Task<IReadOnlyList<FabricNodeReadModel>> ListAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for fabric node stored-procedure result sets.</summary>
public sealed record FabricNodeReadModel
{
    /// <summary>Node identifier.</summary>
    public required Guid NodeId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Node display name.</summary>
    public required string NodeName { get; init; }
    /// <summary>Node type/category.</summary>
    public required string NodeType { get; init; }
    /// <summary>Connectivity status.</summary>
    public required string Status { get; init; }
    /// <summary>UTC timestamp of last heartbeat.</summary>
    public required DateTime LastHeartbeatAtUtc { get; init; }
    /// <summary>Deployment region.</summary>
    public required string Region { get; init; }
}
