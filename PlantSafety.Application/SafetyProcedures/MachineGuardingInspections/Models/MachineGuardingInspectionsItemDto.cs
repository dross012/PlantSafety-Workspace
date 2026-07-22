// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single MachineGuardingInspections record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Models;

/// <summary>A single MachineGuardingInspections record (Machine guarding inspection procedures verify guards are securely mounted, interlocks function correctly, and no guards have been bypassed or removed.).</summary>
public sealed record MachineGuardingInspectionsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid MachineGuardingInspectionsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inspection Title.</summary>
    public required string InspectionTitle { get; init; }

    /// <summary>Classification: GuardMountingCheck, InterlockTest, BypassCheck.</summary>
    public required string InspectionCategory { get; init; }

    /// <summary>Equipment Location.</summary>
    public required string EquipmentLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string InspectionStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}