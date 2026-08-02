// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PMScheduling record. Preventive maintenance scheduling for safety-critical equipment: guards, interlocks, ventilation, sensors, PPE dispensers.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Added CreatedBy/EscalationPriorityCode/CompletionNotes/UpdatedAtUtc/
//   UpdatedBy/ETag (Phase 4 Scheduling.PMScheduling remediation): closes the gap where an API
//   client had no way to read a PMScheduling record's current state (and thus obtain its ETag
//   before issuing a mutating command) by identity. Reused by both the pre-existing date-range
//   list query (GetPMSchedulingQuery, via usp_PMScheduling_GetRange -- which does not populate
//   these new columns) and the new by-identity query (GetPMSchedulingByIdQuery, via
//   usp_PMScheduling_GetById -- which populates all of them). Additive-only: all new properties
//   are optional.

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

    /// <summary>Identity of the user who created this record.</summary>
    public Guid? CreatedBy { get; init; }

    /// <summary>Priority code (P1-P4) assigned when the task is escalated for being overdue.</summary>
    public string? EscalationPriorityCode { get; init; }

    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>UTC timestamp when the record was last updated.</summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who last updated this record.</summary>
    public Guid? UpdatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error. Only populated by the by-identity read path.
    /// </summary>
    public string? ETag { get; init; }
}