// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.StandardsEquipmentRequirement
//   row -- an equipment-specific elaboration of a parent StandardsRequirement. Global reference
//   data, not tenant-scoped.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Standards.Models;

/// <summary>Flat read-model DTO for a single Compliance.StandardsEquipmentRequirement row.</summary>
public sealed record EquipmentRequirementDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid EquipmentRequirementId { get; init; }

    /// <summary>Identity of the parent standards requirement.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>Equipment type this requirement applies to (e.g. "PressureVessel", "Crane").</summary>
    public required string EquipmentType { get; init; }

    /// <summary>Equipment-specific requirement text.</summary>
    public required string RequirementText { get; init; }

    /// <summary>Narrative description of the required inspection interval, when applicable.</summary>
    public string? InspectionIntervalDescription { get; init; }

    /// <summary>Control system category this requirement pertains to, when applicable.</summary>
    public string? ControlSystemCategory { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
