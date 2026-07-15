// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the Drill aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;

/// <summary>Repository for Drill read/write operations.</summary>
public interface IDrillRepository
{
    /// <summary>Creates a new Drill; stages a <c>DrillScheduledEvent</c> in the outbox atomically.</summary>
    Task CreateAsync(
        Guid drillId,
        Guid tenantId,
        Guid siteId,
        string title,
        string drillType,
        DateTimeOffset scheduledAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<DrillReadModel?> GetByIdAsync(Guid tenantId, Guid drillId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of <see cref="DrillReadModel"/> records matching the given filters.
    /// The returned tuple contains the page items and the total count (for pagination metadata).
    /// </summary>
    Task<(IReadOnlyList<DrillReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? drillType,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for Drill stored-procedure result sets.</summary>
public sealed record DrillReadModel
{
    /// <summary>Drill identifier.</summary>
    public required Guid DrillId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description of objectives and scope.</summary>
    public required string Description { get; init; }
    /// <summary>Drill type.</summary>
    public required string DrillType { get; init; }
    /// <summary>Scheduled execution timestamp.</summary>
    public required DateTimeOffset ScheduledAtUtc { get; init; }
    /// <summary>Actual execution timestamp.</summary>
    public required DateTimeOffset? ExecutedAtUtc { get; init; }
    /// <summary>Outcome score (0-100).</summary>
    public required int? OutcomeScore { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Drill coordinator worker identifier.</summary>
    public required Guid CoordinatorId { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
