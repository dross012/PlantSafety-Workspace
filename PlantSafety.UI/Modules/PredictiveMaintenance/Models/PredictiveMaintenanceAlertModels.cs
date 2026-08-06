// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side DTOs for the Operational.PredictiveMaintenance
//   Alert write/lifecycle surface (api/v1/operational/predictive-maintenance/alerts)
//   — Raise -> Review -> ScheduleAction -> Resolve/Dismiss -> Close, plus a real
//   by-id GET. This is a distinct, generic maintenance-alert aggregate from the
//   read-only ML-generated Predictions/Recommendations/Sensor data shown on the
//   other Predictive Maintenance pages — those have no write commands; this is
//   where RaisePredictiveMaintenanceAlertCommand and its lifecycle actions
//   actually live. Mirrors the shapes in
//   PlantSafety.Api.Operational.PredictiveMaintenance.Contracts.Requests.
//   PredictiveMaintenanceAlertRequests and
//   PlantSafety.Application.Operational.PredictiveMaintenance.Models/Commands.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.PredictiveMaintenance.Models;

/// <summary>Full read-model record for a single PredictiveMaintenanceAlert (mirrors <c>PredictiveMaintenanceAlertItemDto</c>), backing GET v1/operational/predictive-maintenance/alerts/{id}.</summary>
public sealed record PredictiveMaintenanceAlertItemDto
{
    /// <summary>Record identifier.</summary>
    public Guid PredictiveMaintenanceAlertId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Raised/UnderReview/ActionScheduled/Resolved/Dismissed/Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Identifier of the equipment being monitored.</summary>
    public Guid EquipmentId { get; init; }

    /// <summary>Predicted failure mode/type.</summary>
    public string PredictedFailureType { get; init; } = string.Empty;

    /// <summary>UTC instant the failure was predicted (when the alert was raised).</summary>
    public DateTime PredictedAtUtc { get; init; }

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
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for raising a new PredictiveMaintenanceAlert (POST v1/operational/predictive-maintenance/alerts). Mirrors <c>RaisePredictiveMaintenanceAlertRequest</c>.</summary>
public sealed class RaisePredictiveMaintenanceAlertRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the identifier of the equipment being monitored.</summary>
    [Required]
    public Guid EquipmentId { get; set; }

    /// <summary>Gets or sets the predicted failure mode/type.</summary>
    [Required]
    [StringLength(400)]
    public string PredictedFailureType { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC instant the failure was predicted.</summary>
    [Required]
    public DateTime PredictedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Result payload shared by the PredictiveMaintenanceAlert commands. Mirrors <c>PredictiveMaintenanceAlertStatusResult</c>.</summary>
public sealed record PredictiveMaintenanceAlertStatusResultDto(Guid PredictiveMaintenanceAlertId, string Status);

/// <summary>Request body carrying only an optimistic-concurrency ETag (review/close).</summary>
public sealed class EtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for scheduling corrective maintenance action (POST .../schedule-action). Mirrors <c>SchedulePredictiveMaintenanceAlertActionRequest</c>.</summary>
public sealed class ScheduleAlertActionRequest
{
    /// <summary>Gets or sets the optional reference to the work order/maintenance ticket raised for this alert.</summary>
    [StringLength(200)]
    public string? WorkOrderReference { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for recording that a predicted failure has been addressed (POST .../resolve). Mirrors <c>ResolvePredictiveMaintenanceAlertRequest</c>.</summary>
public sealed class ResolveAlertRequest
{
    /// <summary>Gets or sets free-text resolution notes.</summary>
    [StringLength(2000)]
    public string? ResolutionNotes { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for dismissing an alert without corrective action (POST .../dismiss). Mirrors <c>DismissPredictiveMaintenanceAlertRequest</c>.</summary>
public sealed class DismissAlertRequest
{
    /// <summary>Gets or sets the reason for dismissal.</summary>
    [Required]
    [StringLength(2000)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
