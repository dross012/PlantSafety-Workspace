// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Cleanup task.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Cleanup.Models;

/// <summary>A single data/system cleanup task record.</summary>
public sealed record CleanupItemDto
{
    /// <summary>Cleanup task identifier.</summary>
    public required Guid CleanupTaskId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Target area the cleanup was scoped to (e.g., TempFiles/StaleSessions/Logs).</summary>
    public required string TargetArea { get; init; }

    /// <summary>Number of items removed by the task.</summary>
    public required int ItemsRemoved { get; init; }

    /// <summary>Storage space reclaimed, in bytes.</summary>
    public required long ReclaimedBytes { get; init; }

    /// <summary>Current task status (e.g., Scheduled/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the task executed.</summary>
    public required DateTime ExecutedAtUtc { get; init; }

    /// <summary>Identity of the user or system that executed the task.</summary>
    public required Guid ExecutedBy { get; init; }
}
