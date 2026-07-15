// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Repository interface for the governed AI/ML model read/write model.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.AiGovernance.Handlers;

/// <summary>Repository for governed AI/ML model read/write operations.</summary>
public interface IAiGovernanceRepository
{
    /// <summary>Registers a new governed model; stages a GovernedModelRegisteredEvent in the outbox.</summary>
    Task CreateAsync(
        Guid modelId,
        Guid tenantId,
        Guid? siteId,
        string modelName,
        string modelVersion,
        string riskTier,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<GovernedModelReadModel?> GetByIdAsync(Guid tenantId, Guid modelId, CancellationToken cancellationToken);

    /// <summary>Returns the governed model registry for a tenant, optionally scoped to a site.</summary>
    Task<IReadOnlyList<GovernedModelReadModel>> ListAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for governed AI/ML model stored-procedure result sets.</summary>
public sealed record GovernedModelReadModel
{
    /// <summary>Model identifier.</summary>
    public required Guid ModelId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Model display name.</summary>
    public required string ModelName { get; init; }
    /// <summary>Model version identifier.</summary>
    public required string ModelVersion { get; init; }
    /// <summary>Governance approval status.</summary>
    public required string GovernanceStatus { get; init; }
    /// <summary>Assessed risk tier.</summary>
    public required string RiskTier { get; init; }
    /// <summary>Current model drift status.</summary>
    public required string DriftStatus { get; init; }
    /// <summary>UTC timestamp of last governance review.</summary>
    public required DateTime LastReviewedAtUtc { get; init; }
    /// <summary>Reviewer display name, or null.</summary>
    public string? ReviewerName { get; init; }
}
