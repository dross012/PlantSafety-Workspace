// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Write-side repository interface for the NearMiss aggregate.
//   Implemented in PlantSafety.Infrastructure via stored procedures and Dapper.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;

/// <summary>Write-side repository for the NearMiss aggregate.</summary>
public interface INearMissRepository
{
    /// <summary>Creates a new NearMiss record; writes an outbox envelope atomically.</summary>
    Task CreateAsync(
        Guid nearMissId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        DateTimeOffset occurredAtUtc,
        string potentialSeverity,
        string hazardCategory,
        Guid reportedByWorkerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Updates mutable fields; only non-null values are applied.</summary>
    Task UpdateAsync(
        Guid tenantId,
        Guid nearMissId,
        string? title,
        string? description,
        string? potentialSeverity,
        string? hazardCategory,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model for the given near miss, or null if not found.</summary>
    Task<NearMissReadModel?> GetByIdAsync(Guid tenantId, Guid nearMissId, CancellationToken cancellationToken);

    /// <summary>Returns a paged list and total count matching the given filters.</summary>
    Task<(IReadOnlyList<NearMissReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? potentialSeverity,
        string? hazardCategory,
        DateTimeOffset? occurredFromUtc,
        DateTimeOffset? occurredToUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for NearMiss stored-procedure result sets.</summary>
public sealed record NearMissReadModel
{
    /// <summary>Near miss identifier.</summary>
    public required Guid NearMissId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>UTC occurrence timestamp.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }
    /// <summary>Potential severity.</summary>
    public required string PotentialSeverity { get; init; }
    /// <summary>Hazard category.</summary>
    public required string HazardCategory { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Reporting worker.</summary>
    public required Guid ReportedByWorkerId { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
