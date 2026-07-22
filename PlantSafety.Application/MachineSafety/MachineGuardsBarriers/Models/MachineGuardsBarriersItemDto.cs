// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single MachineGuardsBarriers record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Models;

/// <summary>A single MachineGuardsBarriers record (Machine guards and barriers are the first line of defense against mechanical hazards: fixed guards, interlocked doors, physical enclosures, and adjustable guards preventing contact with gears, blades, belts, and rollers.).</summary>
public sealed record MachineGuardsBarriersItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid MachineGuardsBarriersId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Guard Title.</summary>
    public required string GuardTitle { get; init; }

    /// <summary>Classification: FixedGuard, InterlockedDoor, PhysicalEnclosure, AdjustableGuard.</summary>
    public required string GuardType { get; init; }

    /// <summary>Equipment Location.</summary>
    public required string EquipmentLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string GuardStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}