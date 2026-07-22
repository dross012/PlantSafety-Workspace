// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single WorkOrderPriority record. Safety work order prioritization (P1 Emergency through P4 Low Priority) with escalation for overdue items.
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.WorkOrderPriority.Models;

/// <summary>A single WorkOrderPriority read-model record.</summary>
public sealed record WorkOrderPriorityItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid WorkOrderPriorityId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>PriorityCode.</summary>
    public required  string PriorityCode { get; init; }

    /// <summary>HazardDescription.</summary>
    public required  string HazardDescription { get; init; }

    /// <summary>DueByUtc.</summary>
    public DateTime? DueByUtc { get; init; }

    /// <summary>EscalatedAtUtc.</summary>
    public DateTime? EscalatedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}