// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PMScheduling record. Preventive maintenance scheduling for safety-critical equipment: guards, interlocks, ventilation, sensors, PPE dispensers.
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.PMScheduling.Models;

/// <summary>A single PMScheduling read-model record.</summary>
public sealed record PMSchedulingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PMSchedulingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>EquipmentId.</summary>
    public required  Guid EquipmentId { get; init; }

    /// <summary>EquipmentDescription.</summary>
    public required  string EquipmentDescription { get; init; }

    /// <summary>PmTaskType.</summary>
    public required  string PmTaskType { get; init; }

    /// <summary>DueAtUtc.</summary>
    public DateTime? DueAtUtc { get; init; }

    /// <summary>IsOverdue.</summary>
    public required  bool IsOverdue { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}