// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           PredictiveMaintenanceAlertItemDto.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single PredictiveMaintenanceAlert record,
//                  including every aggregate property plus an ETag. Backs
//                  GetPredictiveMaintenanceAlertByIdQuery. Distinct from the pre-existing, simpler
//                  PredictiveMaintenanceItemDto used by the list-range query (GetPredictiveMaintenanceQuery),
//                  which is left untouched. Mirrors PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.PredictiveMaintenance.Models;

/// <summary>A single PredictiveMaintenanceAlert read-model record.</summary>
public sealed record PredictiveMaintenanceAlertItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PredictiveMaintenanceAlertId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Raised/UnderReview/ActionScheduled/Resolved/Dismissed/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>Identifier of the equipment being monitored.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Predicted failure mode/type.</summary>
    public required string PredictedFailureType { get; init; }

    /// <summary>UTC instant the failure was predicted (when the alert was raised).</summary>
    public required DateTime PredictedAtUtc { get; init; }

    /// <summary>Identity of the reviewer who triaged this alert.</summary>
    public Guid? ReviewedBy { get; init; }

    /// <summary>UTC instant the alert was reviewed.</summary>
    public DateTime? ReviewedAtUtc { get; init; }

    /// <summary>Identity of the user who scheduled corrective action.</summary>
    public Guid? ActionScheduledBy { get; init; }

    /// <summary>Optional reference to the work order/maintenance ticket raised for this alert.</summary>
    public string? WorkOrderReference { get; init; }

    /// <summary>UTC instant corrective action was scheduled.</summary>
    public DateTime? ActionScheduledAtUtc { get; init; }

    /// <summary>Identity of the user who resolved this alert.</summary>
    public Guid? ResolvedBy { get; init; }

    /// <summary>Free-text resolution notes.</summary>
    public string? ResolutionNotes { get; init; }

    /// <summary>UTC instant the alert was resolved.</summary>
    public DateTime? ResolvedAtUtc { get; init; }

    /// <summary>Identity of the user who dismissed this alert.</summary>
    public Guid? DismissedBy { get; init; }

    /// <summary>Reason recorded when an alert is dismissed without corrective action.</summary>
    public string? DismissedReason { get; init; }

    /// <summary>UTC instant the alert was dismissed.</summary>
    public DateTime? DismissedAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed this alert.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>UTC instant the alert was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

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
