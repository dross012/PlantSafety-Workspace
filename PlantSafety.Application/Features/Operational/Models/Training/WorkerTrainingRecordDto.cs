// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Worker Training Record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Models.Training;

/// <summary>
/// Read-side projection of a WorkerTrainingRecord.
/// Records completion of a safety training course by a worker.
/// </summary>
public sealed record WorkerTrainingRecordDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid TrainingRecordId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Worker who completed the training.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Training course identifier.</summary>
    public required Guid CourseId { get; init; }

    /// <summary>Training course name.</summary>
    public required string CourseName { get; init; }

    /// <summary>Delivery method (e.g., InPerson, Online, OJT).</summary>
    public required string DeliveryMethod { get; init; }

    /// <summary>Completion status (e.g., Assigned, InProgress, Completed, Failed, Expired).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when training was assigned.</summary>
    public required DateTimeOffset AssignedAtUtc { get; init; }

    /// <summary>UTC timestamp when training was completed, if applicable.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }

    /// <summary>UTC timestamp when this training certification expires, if applicable.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; init; }

    /// <summary>Pass/fail score, if applicable (0–100).</summary>
    public int? Score { get; init; }

    /// <summary>Instructor or trainer, if applicable.</summary>
    public Guid? InstructorId { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
}
