// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single MachineGuarding record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.MachineGuarding.Models;

/// <summary>A single MachineGuarding record (Machine guarding prevents workers from contacting hazardous moving parts, pinch points, sharp edges, and high-energy mechanisms via fixed guards, interlocked doors, adjustable guards, and presence-sensing devices.).</summary>
public sealed record MachineGuardingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid MachineGuardingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Guard Title.</summary>
    public required string GuardTitle { get; init; }

    /// <summary>Classification: FixedGuard, InterlockedDoor, AdjustableGuard, PresenceSensingDevice.</summary>
    public required string GuardType { get; init; }

    /// <summary>Location.</summary>
    public required string Location { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string GuardStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}