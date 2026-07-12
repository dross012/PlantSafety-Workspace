// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single PredictiveMaintenance record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Models;

/// <summary>A single PredictiveMaintenance read-model record.</summary>
public sealed record PredictiveMaintenanceItemDto
{
    /// <summary>PredictiveMaintenance record identifier.</summary>
    public required Guid PredictiveMaintenanceAlertId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment being monitored.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Predicted failure mode.</summary>
    public required string PredictedFailureType { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime PredictedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid ReviewedBy { get; init; }
}
