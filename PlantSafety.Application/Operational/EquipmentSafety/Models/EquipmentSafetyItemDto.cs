// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single EquipmentSafety record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.EquipmentSafety.Models;

/// <summary>A single EquipmentSafety read-model record.</summary>
public sealed record EquipmentSafetyItemDto
{
    /// <summary>EquipmentSafety record identifier.</summary>
    public required Guid EquipmentSafetyRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment item.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Display name of the equipment item.</summary>
    public required string EquipmentName { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime InspectedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid InspectedBy { get; init; }
}
