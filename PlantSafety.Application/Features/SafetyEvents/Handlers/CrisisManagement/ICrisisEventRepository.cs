// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the CrisisEvent aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;

/// <summary>Repository for CrisisEvent read/write operations.</summary>
public interface ICrisisEventRepository
{
    /// <summary>Creates a new CrisisEvent; stages a high-priority <c>CrisisDeclaredEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid crisisEventId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string crisisCategory,
        Guid incidentCommanderId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<CrisisEventReadModel?> GetByIdAsync(Guid tenantId, Guid crisisEventId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for CrisisEvent stored-procedure result sets.</summary>
public sealed record CrisisEventReadModel
{
    /// <summary>Crisis event identifier.</summary>
    public required Guid CrisisEventId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Crisis category.</summary>
    public required string CrisisCategory { get; init; }
    /// <summary>Incident commander worker identifier.</summary>
    public required Guid IncidentCommanderId { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
