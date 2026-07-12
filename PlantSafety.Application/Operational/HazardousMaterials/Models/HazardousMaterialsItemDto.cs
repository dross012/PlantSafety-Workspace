// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single HazardousMaterials record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.HazardousMaterials.Models;

/// <summary>A single HazardousMaterials read-model record.</summary>
public sealed record HazardousMaterialsItemDto
{
    /// <summary>HazardousMaterials record identifier.</summary>
    public required Guid HazardousMaterialId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the hazardous material.</summary>
    public required string MaterialName { get; init; }

    /// <summary>Hazard classification (e.g., GHS class).</summary>
    public required string HazardClass { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime RegisteredAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid RegisteredBy { get; init; }
}
