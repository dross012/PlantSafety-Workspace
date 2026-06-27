// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the WorkerTrainingRecord aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Handlers.Training;

/// <summary>Repository for WorkerTrainingRecord read/write operations.</summary>
public interface ITrainingRepository
{
    /// <summary>Creates a new training assignment; stages a <c>TrainingAssignedEvent</c>.</summary>
    Task AssignAsync(
        Guid recordId,
        Guid tenantId,
        Guid workerId,
        string trainingCourseCode,
        string trainingCourseName,
        string deliveryMethod,
        DateTimeOffset dueDateUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<TrainingReadModel?> GetByIdAsync(Guid tenantId, Guid recordId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for WorkerTrainingRecord stored-procedure result sets.</summary>
public sealed record TrainingReadModel
{
    /// <summary>Training record identifier.</summary>
    public required Guid RecordId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Worker identifier.</summary>
    public required Guid WorkerId { get; init; }
    /// <summary>Training course code.</summary>
    public required string TrainingCourseCode { get; init; }
    /// <summary>Training course name.</summary>
    public required string TrainingCourseName { get; init; }
    /// <summary>Delivery method.</summary>
    public required string DeliveryMethod { get; init; }
    /// <summary>Status: Assigned/InProgress/Completed/Failed/Expired.</summary>
    public required string Status { get; init; }
    /// <summary>Due date.</summary>
    public required DateTimeOffset DueDateUtc { get; init; }
    /// <summary>Completion date.</summary>
    public required DateTimeOffset? CompletedAtUtc { get; init; }
    /// <summary>Expiry date.</summary>
    public required DateTimeOffset? ExpiresAtUtc { get; init; }
    /// <summary>Score (0-100).</summary>
    public required int? Score { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
