// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PPEAssignment record. PPE assignment and compliance: worker/role/zone-based PPE assignment profiles and compliance status.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.PPEAssignment.Models;

/// <summary>A single PPEAssignment read-model record.</summary>
public sealed record PPEAssignmentItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PPEAssignmentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>WorkerId.</summary>
    public required  Guid WorkerId { get; init; }

    /// <summary>JobRole.</summary>
    public required  string JobRole { get; init; }

    /// <summary>HazardZone.</summary>
    public required  string HazardZone { get; init; }

    /// <summary>AssignedPpeItems.</summary>
    public required  string AssignedPpeItems { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}