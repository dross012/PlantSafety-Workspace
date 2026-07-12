// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Update record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Updates.Models;

/// <summary>A single system/component update record.</summary>
public sealed record UpdatesItemDto
{
    /// <summary>Update identifier.</summary>
    public required Guid UpdateId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the component that was updated.</summary>
    public required string ComponentName { get; init; }

    /// <summary>Version the component was updated from.</summary>
    public required string VersionFrom { get; init; }

    /// <summary>Version the component was updated to.</summary>
    public required string VersionTo { get; init; }

    /// <summary>Current update status (e.g., Scheduled/Applying/Applied/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the update was applied.</summary>
    public required DateTime AppliedAtUtc { get; init; }

    /// <summary>Identity of the user or system that applied the update.</summary>
    public required Guid AppliedBy { get; init; }
}
