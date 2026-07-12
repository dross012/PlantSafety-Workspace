// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single PreventiveMaintenance record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Models;

/// <summary>A single PreventiveMaintenance read-model record.</summary>
public sealed record PreventiveMaintenanceItemDto
{
    /// <summary>PreventiveMaintenance record identifier.</summary>
    public required Guid PreventiveMaintenanceTaskId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment scheduled for maintenance.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Description of the maintenance task.</summary>
    public required string TaskDescription { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime ScheduledAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid CompletedBy { get; init; }
}
