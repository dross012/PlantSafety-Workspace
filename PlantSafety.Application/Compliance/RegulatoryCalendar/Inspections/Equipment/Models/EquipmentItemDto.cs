// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Equipment record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.Equipment.Models;

/// <summary>A single Equipment read-model item.</summary>
public sealed record EquipmentItemDto
{
    /// <summary>Inspection Id identifier.</summary>
    public required Guid InspectionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Equipment Category.</summary>
    public required string EquipmentCategory { get; init; }

    /// <summary>Scheduled At Utc.</summary>
    public required DateTime ScheduledAtUtc { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Inspector.</summary>
    public required Guid Inspector { get; init; }
}
