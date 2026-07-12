// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single LockoutTagout record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.LockoutTagout.Models;

/// <summary>A single LockoutTagout read-model record.</summary>
public sealed record LockoutTagoutItemDto
{
    /// <summary>LockoutTagout record identifier.</summary>
    public required Guid LockoutTagoutId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment under lockout/tagout.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Type of lock applied (e.g., Electrical/Mechanical).</summary>
    public required string LockType { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime LockedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid LockedBy { get; init; }
}
