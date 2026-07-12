// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single employee training record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Certification.Training.Models;

/// <summary>A single employee training completion record.</summary>
public sealed record TrainingItemDto
{
    /// <summary>Training record identifier.</summary>
    public required Guid TrainingRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the trained employee.</summary>
    public required string EmployeeName { get; init; }

    /// <summary>Name of the training course completed.</summary>
    public required string CourseName { get; init; }

    /// <summary>Current training status (e.g., Completed/Expired/InProgress).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the training was completed.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>UTC timestamp when the training certification expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the instructor who delivered the training.</summary>
    public required Guid InstructorId { get; init; }
}
