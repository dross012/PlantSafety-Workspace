// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Training record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Training.Models;

/// <summary>A single Training read-model record.</summary>
public sealed record TrainingItemDto
{
    /// <summary>Training record identifier.</summary>
    public required Guid TrainingRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee this training record relates to.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Name of the training course.</summary>
    public required string CourseName { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid CertifiedBy { get; init; }
}
