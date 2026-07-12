// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single CompetencyManagement record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.CompetencyManagement.Models;

/// <summary>A single CompetencyManagement read-model record.</summary>
public sealed record CompetencyManagementItemDto
{
    /// <summary>CompetencyManagement record identifier.</summary>
    public required Guid CompetencyRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee this competency record relates to.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Competency area being assessed.</summary>
    public required string CompetencyArea { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime AssessedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid AssessedBy { get; init; }
}
