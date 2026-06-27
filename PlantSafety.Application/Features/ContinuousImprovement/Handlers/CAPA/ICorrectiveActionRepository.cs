// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the CorrectiveAction aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Repository for CorrectiveAction read/write operations.</summary>
public interface ICorrectiveActionRepository
{
    /// <summary>Creates a new CorrectiveAction; stages a <c>CorrectiveActionCreatedEvent</c>.</summary>
    Task CreateAsync(
        Guid caId,
        Guid tenantId,
        Guid sourceEventId,
        string sourceEventType,
        string title,
        string description,
        string actionType,
        string priority,
        Guid assignedToWorkerId,
        DateTimeOffset dueAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<CorrectiveActionReadModel?> GetByIdAsync(Guid tenantId, Guid caId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<CorrectiveActionReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? sourceEventId,
        string? actionType,
        string? priority,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for CorrectiveAction stored-procedure result sets.</summary>
public sealed record CorrectiveActionReadModel
{
    /// <summary>Corrective action identifier.</summary>
    public required Guid CaId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Source event identifier.</summary>
    public required Guid SourceEventId { get; init; }
    /// <summary>Source event type discriminator.</summary>
    public required string SourceEventType { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Action type: Corrective or Preventive.</summary>
    public required string ActionType { get; init; }
    /// <summary>Priority.</summary>
    public required string Priority { get; init; }
    /// <summary>Assigned worker identifier.</summary>
    public required Guid AssignedToWorkerId { get; init; }
    /// <summary>Due date.</summary>
    public required DateTimeOffset DueAtUtc { get; init; }
    /// <summary>Status.</summary>
    public required string Status { get; init; }
    /// <summary>Completion date.</summary>
    public required DateTimeOffset? CompletedAtUtc { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
