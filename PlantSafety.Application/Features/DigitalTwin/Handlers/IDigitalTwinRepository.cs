// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Repository interface for the DigitalTwin asset read/write model.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.DigitalTwin.Handlers;

/// <summary>Repository for Digital Twin asset read/write operations.</summary>
public interface IDigitalTwinRepository
{
    /// <summary>Registers a new Digital Twin; stages a <c>DigitalTwinRegisteredEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid twinId,
        Guid tenantId,
        Guid? siteId,
        string assetName,
        string assetType,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<DigitalTwinReadModel?> GetByIdAsync(Guid tenantId, Guid twinId, CancellationToken cancellationToken);

    /// <summary>Returns the twin registry for a tenant, optionally scoped to a site.</summary>
    Task<IReadOnlyList<DigitalTwinReadModel>> ListAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for DigitalTwin stored-procedure result sets.</summary>
public sealed record DigitalTwinReadModel
{
    /// <summary>Twin identifier.</summary>
    public required Guid TwinId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Asset display name.</summary>
    public required string AssetName { get; init; }
    /// <summary>Asset type/category.</summary>
    public required string AssetType { get; init; }
    /// <summary>Synchronization status.</summary>
    public required string Status { get; init; }
    /// <summary>UTC timestamp of last telemetry sync.</summary>
    public required DateTime LastSyncAtUtc { get; init; }
    /// <summary>Telemetry health score (0.0-1.0).</summary>
    public required double TelemetryHealthScore { get; init; }
}
