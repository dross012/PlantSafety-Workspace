// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single SiteOrientation record.
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.SiteOrientation.Models;

/// <summary>A single SiteOrientation record (Site-specific orientation trains contractors on the facility's unique hazards: plant layout, restricted zones, emergency signals/evacuation, chemical hazards/SDS access, machine guarding rules, and PPE requirements.).</summary>
public sealed record SiteOrientationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid SiteOrientationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Orientation Title.</summary>
    public required string OrientationTitle { get; init; }

    /// <summary>Classification: PlantLayout, EmergencyProcedures, ChemicalHazards, MachineGuardingRules, PpeRequirements.</summary>
    public required string OrientationCategory { get; init; }

    /// <summary>Contractor Name.</summary>
    public required string ContractorName { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string OrientationStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}