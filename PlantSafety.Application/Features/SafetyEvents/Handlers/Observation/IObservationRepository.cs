// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the Observation aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;

/// <summary>Repository for Observation read/write operations.</summary>
public interface IObservationRepository
{
    /// <summary>Creates a new Observation; stages an outbox event atomically.</summary>
    Task CreateAsync(
        Guid observationId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string observationType,
        DateTimeOffset observedAtUtc,
        Guid observedByWorkerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Updates mutable fields; only non-null values are applied.</summary>
    Task UpdateAsync(
        Guid tenantId,
        Guid observationId,
        string? title,
        string? description,
        string? observationType,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<ObservationReadModel?> GetByIdAsync(Guid tenantId, Guid observationId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<ObservationReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? observationType,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for Observation stored-procedure result sets.</summary>
public sealed record ObservationReadModel
{
    /// <summary>Observation identifier.</summary>
    public required Guid ObservationId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Observation type: Positive, AtRisk, or Unsafe.</summary>
    public required string ObservationType { get; init; }
    /// <summary>UTC observation timestamp.</summary>
    public required DateTimeOffset ObservedAtUtc { get; init; }
    /// <summary>Observer worker identifier.</summary>
    public required Guid ObservedByWorkerId { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
