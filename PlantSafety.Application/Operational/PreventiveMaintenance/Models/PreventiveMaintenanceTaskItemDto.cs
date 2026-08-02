// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           PreventiveMaintenanceTaskItemDto.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single PreventiveMaintenanceTask record. Backs
//                  GetPreventiveMaintenanceTaskByIdQuery. Distinct from the pre-existing simpler
//                  PreventiveMaintenanceItemDto used by GetPreventiveMaintenanceQuery (the
//                  date-range list query) -- that DTO and its query/handler/repository are left
//                  untouched by this build-out. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.PreventiveMaintenance.Models;

/// <summary>A single PreventiveMaintenanceTask read-model record.</summary>
public sealed record PreventiveMaintenanceTaskItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PreventiveMaintenanceTaskId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Scheduled/InProgress/Completed/Verified/Cancelled/Overdue).</summary>
    public required string Status { get; init; }

    /// <summary>Identifier of the equipment scheduled for maintenance.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Description of the maintenance task.</summary>
    public required string TaskDescription { get; init; }

    /// <summary>UTC instant the task is/was scheduled for.</summary>
    public required DateTime ScheduledAtUtc { get; init; }

    /// <summary>Identity of the user who started the maintenance work.</summary>
    public Guid? StartedBy { get; init; }

    /// <summary>UTC instant maintenance work began.</summary>
    public DateTime? StartedAtUtc { get; init; }

    /// <summary>Identity of the user who completed the maintenance work.</summary>
    public Guid? CompletedBy { get; init; }

    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>UTC instant maintenance work was completed.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who verified the completed work.</summary>
    public Guid? VerifiedBy { get; init; }

    /// <summary>UTC instant the completed work was verified.</summary>
    public DateTime? VerifiedAtUtc { get; init; }

    /// <summary>Identity of the user who cancelled the task.</summary>
    public Guid? CancelledBy { get; init; }

    /// <summary>Reason recorded when a task is cancelled.</summary>
    public string? CancelledReason { get; init; }

    /// <summary>UTC instant the task was cancelled.</summary>
    public DateTime? CancelledAtUtc { get; init; }

    /// <summary>UTC instant the task was marked overdue.</summary>
    public DateTime? OverdueAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
