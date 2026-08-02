// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single WorkOrderPriority record. Safety work order prioritization (P1 Emergency through P4 Low Priority) with escalation for overdue items.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Added CreatedBy/ResolutionNotes/UpdatedAtUtc/UpdatedBy/ETag (Phase 4
//   Scheduling.WorkOrderPriority remediation): closes the gap where an API client had no way to
//   read a WorkOrderPriority record's current state (and thus obtain its ETag before issuing a
//   mutating command) by identity. Reused by both the pre-existing date-range list query
//   (GetWorkOrderPriorityQuery, via usp_WorkOrderPriority_GetRange -- which does not populate
//   these new columns) and the new by-identity query (GetWorkOrderPriorityByIdQuery, via
//   usp_WorkOrderPriority_GetById -- which populates all of them). Additive-only: all new
//   properties are optional.

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

    /// <summary>Identity of the user who created this record.</summary>
    public Guid? CreatedBy { get; init; }

    /// <summary>Free-text notes recorded when the hazard is resolved.</summary>
    public string? ResolutionNotes { get; init; }

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