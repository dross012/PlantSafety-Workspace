// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Backup job.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Backup.Models;

/// <summary>A single backup job record.</summary>
public sealed record BackupItemDto
{
    /// <summary>Backup job identifier.</summary>
    public required Guid BackupJobId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Backup type (e.g., Full/Incremental/Differential).</summary>
    public required string BackupType { get; init; }

    /// <summary>Storage location the backup was written to.</summary>
    public required string TargetLocation { get; init; }

    /// <summary>Size of the backup artifact, in bytes.</summary>
    public required long SizeBytes { get; init; }

    /// <summary>Current job status (e.g., Queued/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the job completed.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user or system that initiated the job.</summary>
    public required Guid InitiatedBy { get; init; }
}
