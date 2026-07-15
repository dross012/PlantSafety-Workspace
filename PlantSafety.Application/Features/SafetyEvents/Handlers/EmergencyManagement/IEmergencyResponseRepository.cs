// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the EmergencyResponse aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;

/// <summary>Repository for EmergencyResponse read/write operations.</summary>
public interface IEmergencyResponseRepository
{
    /// <summary>Creates a new EmergencyResponse; stages an outbox event atomically.</summary>
    Task CreateAsync(
        Guid responseId,
        Guid tenantId,
        Guid siteId,
        Guid? linkedCrisisEventId,
        string responsePlanName,
        string description,
        DateTimeOffset activatedAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<EmergencyResponseReadModel?> GetByIdAsync(Guid tenantId, Guid responseId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of <see cref="EmergencyResponseReadModel"/> records matching the given filters.
    /// The returned tuple contains the page items and the total count (for pagination metadata).
    /// </summary>
    Task<(IReadOnlyList<EmergencyResponseReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for EmergencyResponse stored-procedure result sets.</summary>
public sealed record EmergencyResponseReadModel
{
    /// <summary>Response identifier.</summary>
    public required Guid ResponseId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Linked crisis event identifier.</summary>
    public required Guid? LinkedCrisisEventId { get; init; }
    /// <summary>Response plan name.</summary>
    public required string ResponsePlanName { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Activation timestamp.</summary>
    public required DateTimeOffset ActivatedAtUtc { get; init; }
    /// <summary>Stand-down timestamp.</summary>
    public required DateTimeOffset? StandDownAtUtc { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Response coordinator worker identifier.</summary>
    public required Guid CoordinatorId { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
