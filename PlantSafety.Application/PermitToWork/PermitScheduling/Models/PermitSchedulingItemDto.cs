// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PermitScheduling record. Scheduling integration for permits: conflict detection (e.g. hot work near flammables) and contractor coordination.
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.PermitScheduling.Models;

/// <summary>A single PermitScheduling read-model record.</summary>
public sealed record PermitSchedulingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PermitSchedulingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>PermitNumber.</summary>
    public required  string PermitNumber { get; init; }

    /// <summary>ScheduledStartUtc.</summary>
    public DateTime? ScheduledStartUtc { get; init; }

    /// <summary>ScheduledEndUtc.</summary>
    public DateTime? ScheduledEndUtc { get; init; }

    /// <summary>ConflictDetected.</summary>
    public required  bool ConflictDetected { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}