// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Restore job.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Restore.Models;

/// <summary>A single data restore job record.</summary>
public sealed record RestoreItemDto
{
    /// <summary>Restore job identifier.</summary>
    public required Guid RestoreJobId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the backup this restore was sourced from.</summary>
    public required Guid SourceBackupId { get; init; }

    /// <summary>Restore type (e.g., FullRestore/PointInTime/PartialRestore).</summary>
    public required string RestoreType { get; init; }

    /// <summary>Target location the data was restored to.</summary>
    public required string TargetLocation { get; init; }

    /// <summary>Current job status (e.g., Queued/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the job completed.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user or system that initiated the job.</summary>
    public required Guid InitiatedBy { get; init; }
}
