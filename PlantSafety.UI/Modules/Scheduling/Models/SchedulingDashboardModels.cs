// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Scheduling Dashboard page
//   (Volume S1 §1). Job/Resource summaries and the Upcoming Jobs list row.
//   Display-only — no domain logic. Additive-only evolution.
// SchemaVersion: 2026.1
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Scheduling.Models;

/// <summary>Job Summary card counts (Pending/In Progress/Completed).</summary>
public sealed class JobSummaryDto
{
    /// <summary>Gets the count of pending jobs.</summary>
    public int PendingCount { get; init; }

    /// <summary>Gets the count of jobs in progress.</summary>
    public int InProgressCount { get; init; }

    /// <summary>Gets the count of completed jobs.</summary>
    public int CompletedCount { get; init; }
}

/// <summary>Resource Summary card counts (Active/Overallocated/Idle).</summary>
public sealed class ResourceSummaryDto
{
    /// <summary>Gets the count of active resources.</summary>
    public int ActiveCount { get; init; }

    /// <summary>Gets the count of overallocated resources.</summary>
    public int OverallocatedCount { get; init; }

    /// <summary>Gets the count of idle resources.</summary>
    public int IdleCount { get; init; }
}

/// <summary>Single row in the Upcoming Jobs table.</summary>
public sealed class UpcomingJobDto
{
    /// <summary>Gets the job identifier (UUIDv7).</summary>
    public Guid JobId { get; init; }

    /// <summary>Gets the human-readable job code (format J-###).</summary>
    public string JobCode { get; init; } = string.Empty;

    /// <summary>Gets the assigned resource display name.</summary>
    public string ResourceName { get; init; } = string.Empty;

    /// <summary>Gets the scheduled start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the scheduled end time (UTC).</summary>
    public DateTimeOffset EndUtc { get; init; }

    /// <summary>Gets the job status code (Pending, InProgress, Completed).</summary>
    public string Status { get; init; } = string.Empty;
}
