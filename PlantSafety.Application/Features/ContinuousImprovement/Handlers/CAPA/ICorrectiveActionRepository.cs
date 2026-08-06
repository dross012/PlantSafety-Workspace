// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-06 -- CreateAsync(primitives) replaced by AddAsync(CorrectiveAction aggregate)
// so the real Domain.ContinuousImprovement.CAPA.Aggregates.CorrectiveAction aggregate (and its
// CorrectiveActionCreatedEvent) is actually constructed and persisted, instead of the handler
// pushing raw primitives straight to SQL with no aggregate/domain event in the loop at all.
// UPDATED: 2026-08-06 -- added GetAggregateByIdAsync/UpdateAsync so the Start/Complete/Close/
// MarkOverdue lifecycle commands can load, transition, and persist the real aggregate (mirrors
// IIncidentRepository's read-then-lifecycle-method-then-persist pattern).
// SchemaVersion: 3

using PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Repository for CorrectiveAction read/write operations.</summary>
public interface ICorrectiveActionRepository
{
    /// <summary>Persists a newly-created <see cref="CorrectiveAction"/> aggregate and relays its pending domain events.</summary>
    Task AddAsync(CorrectiveAction aggregate, Guid actorId, Guid correlationId, CancellationToken cancellationToken);

    /// <summary>Persists a lifecycle transition on an existing <see cref="CorrectiveAction"/> aggregate and relays its pending domain events.</summary>
    Task UpdateAsync(CorrectiveAction aggregate, Guid actorId, Guid correlationId, CancellationToken cancellationToken);

    /// <summary>Loads and rehydrates the <see cref="CorrectiveAction"/> aggregate, or null if not found.</summary>
    Task<CorrectiveAction?> GetAggregateByIdAsync(Guid tenantId, Guid caId, CancellationToken cancellationToken);

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
