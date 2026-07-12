// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single analytics report schedule.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Schedules.Models;

/// <summary>A single scheduled analytics job or recurring report definition.</summary>
public sealed record SchedulesItemDto
{
    /// <summary>Schedule item identifier.</summary>
    public required Guid ScheduleItemId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the schedule.</summary>
    public required string ScheduleName { get; init; }

    /// <summary>Recurrence frequency (e.g., Daily, Weekly, Monthly).</summary>
    public required string Frequency { get; init; }

    /// <summary>UTC timestamp of the next scheduled run.</summary>
    public required DateTime NextRunAtUtc { get; init; }

    /// <summary>Current status of the schedule (e.g., Active, Paused, Disabled).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the schedule was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created the schedule.</summary>
    public required Guid CreatedBy { get; init; }
}
