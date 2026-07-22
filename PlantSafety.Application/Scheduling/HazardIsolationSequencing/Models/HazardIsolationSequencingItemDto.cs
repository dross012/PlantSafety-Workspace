// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single HazardIsolationSequencing record. Isolation and sequencing of incompatible hazardous tasks (hot work, chemical handling, heavy lifting) within hazard zones.
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models;

/// <summary>A single HazardIsolationSequencing read-model record.</summary>
public sealed record HazardIsolationSequencingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazardIsolationSequencingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>HazardZone.</summary>
    public required  string HazardZone { get; init; }

    /// <summary>TaskType.</summary>
    public required  string TaskType { get; init; }

    /// <summary>WindowStartUtc.</summary>
    public DateTime? WindowStartUtc { get; init; }

    /// <summary>WindowEndUtc.</summary>
    public DateTime? WindowEndUtc { get; init; }

    /// <summary>ConflictDetected.</summary>
    public required  bool ConflictDetected { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}