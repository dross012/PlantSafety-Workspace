// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single FatigueManagement record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.FatigueManagement.Models;

/// <summary>A single FatigueManagement read-model record.</summary>
public sealed record FatigueManagementItemDto
{
    /// <summary>FatigueManagement record identifier.</summary>
    public required Guid FatigueRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee this fatigue record relates to.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Fatigue risk level classification.</summary>
    public required string FatigueRiskLevel { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid RecordedBy { get; init; }
}
