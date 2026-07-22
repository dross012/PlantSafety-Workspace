// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single TwoHandControls record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.TwoHandControls.Models;

/// <summary>A single TwoHandControls record (Two-hand control systems require operators to press two separate buttons simultaneously to activate machine cycles, keeping hands outside the danger zone on presses, cutters, and forming machines.).</summary>
public sealed record TwoHandControlsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid TwoHandControlsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Control Title.</summary>
    public required string ControlTitle { get; init; }

    /// <summary>Classification: Press, Cutter, FormingMachine.</summary>
    public required string ControlType { get; init; }

    /// <summary>Equipment Location.</summary>
    public required string EquipmentLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ControlStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}