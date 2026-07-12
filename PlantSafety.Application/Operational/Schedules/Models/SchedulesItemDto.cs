// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Schedules record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Schedules.Models;

/// <summary>A single Schedules read-model record.</summary>
public sealed record SchedulesItemDto
{
    /// <summary>Schedules record identifier.</summary>
    public required Guid ScheduleId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the shift.</summary>
    public required string ShiftName { get; init; }

    /// <summary>Type of schedule (e.g., Shift/Rotation).</summary>
    public required string ScheduleType { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime ScheduledAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid AssignedBy { get; init; }
}
