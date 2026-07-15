// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Repository interface for the ProcessDefinition aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ProcessManagement.Handlers.Processes;

/// <summary>Repository for ProcessDefinition read/write operations.</summary>
public interface IProcessRepository
{
    /// <summary>Creates a new ProcessDefinition; stages a <c>ProcessDefinitionCreatedEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid processId,
        Guid tenantId,
        Guid? siteId,
        string processName,
        string category,
        Guid ownerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<ProcessReadModel?> GetByIdAsync(Guid tenantId, Guid processId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<ProcessReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? category,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for ProcessDefinition stored-procedure result sets.</summary>
public sealed record ProcessReadModel
{
    /// <summary>Process identifier.</summary>
    public required Guid ProcessId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Human-readable process name.</summary>
    public required string ProcessName { get; init; }
    /// <summary>Safety-critical process category.</summary>
    public required string Category { get; init; }
    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }
    /// <summary>Monotonically increasing revision number.</summary>
    public required int Version { get; init; }
    /// <summary>Identity of the accountable process owner.</summary>
    public required Guid OwnerId { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
