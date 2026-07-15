// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Application-layer repository interface for the WorkerWellBeing read model.
//   Distinct from PlantSafety.Domain.WorkerWellBeing.Repositories.IWorkerWellBeingRepository
//   (the aggregate-oriented domain repository); this interface serves the CQRS read/write
//   slice backing the Features/WorkerWellBeing Application handlers.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.WorkerWellBeing.Handlers.Records;

/// <summary>Repository for WorkerWellBeing record read/write operations.</summary>
public interface IWorkerWellBeingRecordRepository
{
    /// <summary>Returns the read model, or null if not found.</summary>
    Task<WorkerWellBeingReadModel?> GetByIdAsync(Guid tenantId, Guid workerWellBeingId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<WorkerWellBeingReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? workerId,
        string? wellBeingRiskLevel,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for WorkerWellBeing stored-procedure result sets.</summary>
public sealed record WorkerWellBeingReadModel
{
    /// <summary>Record identifier.</summary>
    public required Guid WorkerWellBeingId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Worker identifier.</summary>
    public required Guid WorkerId { get; init; }
    /// <summary>Wellbeing risk classification.</summary>
    public required string WellBeingRiskLevel { get; init; }
    /// <summary>Workflow status.</summary>
    public required string Status { get; init; }
    /// <summary>Recorded/last-actioned timestamp.</summary>
    public required DateTimeOffset RecordedAtUtc { get; init; }
}
