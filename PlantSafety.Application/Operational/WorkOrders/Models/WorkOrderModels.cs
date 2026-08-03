// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderModels.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-side DTOs backing the Maintenance UI's work-order core
//                  (v1/maintenance/work-orders*, machines/*, technicians/*, calendar). Consolidated
//                  into one file per the sibling PreventiveMaintenanceTaskEvents.cs precedent of
//                  grouping closely-related small record types. Field shapes are grounded in
//                  PlantSafety.UI.Modules.Maintenance.Models.MaintenanceModels.cs (the pre-existing
//                  UI-side DTOs this module now backs for real). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.WorkOrders.Models;

/// <summary>Work Order Summary card counts (Open/Assigned/InProgress/OnHold/Completed/Cancelled).</summary>
public sealed record WorkOrderSummaryDto
{
    /// <summary>Count of work orders in Open status.</summary>
    public required int OpenCount { get; init; }

    /// <summary>Count of work orders in InProgress status (Assigned + InProgress + OnHold, matching the UI's "In Progress" bucket).</summary>
    public required int InProgressCount { get; init; }

    /// <summary>Count of work orders in Completed status.</summary>
    public required int CompletedCount { get; init; }
}

/// <summary>
/// Machine Status Summary card counts. Derived heuristically from open work-order priority per
/// distinct MachineId, since no Machine/Equipment master aggregate exists in the platform yet
/// (assumption, documented here and in the repository implementation).
/// </summary>
public sealed record MachineStatusSummaryDto
{
    /// <summary>Count of machines with no open work order.</summary>
    public required int HealthyCount { get; init; }

    /// <summary>Count of machines with an open, non-Critical-priority work order.</summary>
    public required int DegradedCount { get; init; }

    /// <summary>Count of machines with an open Critical-priority work order.</summary>
    public required int OfflineCount { get; init; }
}

/// <summary>Single row in the Active Work Orders table.</summary>
public sealed record WorkOrderRowDto
{
    /// <summary>Work order identifier.</summary>
    public required Guid WorkOrderId { get; init; }

    /// <summary>Human-readable work order code (format WO-###).</summary>
    public required string WorkOrderCode { get; init; }

    /// <summary>Machine identifier this work order targets.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Human-readable machine code (format M-###).</summary>
    public required string MachineCode { get; init; }

    /// <summary>Assigned technician display name (empty when unassigned).</summary>
    public required string TechnicianName { get; init; }

    /// <summary>Work order priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Work order status.</summary>
    public required string Status { get; init; }
}

/// <summary>Full work order detail header.</summary>
public sealed record WorkOrderItemDto
{
    /// <summary>Work order identifier.</summary>
    public required Guid WorkOrderId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Human-readable work order code.</summary>
    public required string WorkOrderCode { get; init; }

    /// <summary>Target machine identifier.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Human-readable machine code.</summary>
    public required string MachineCode { get; init; }

    /// <summary>Assigned technician display name (empty when unassigned).</summary>
    public required string TechnicianName { get; init; }

    /// <summary>Work order status.</summary>
    public required string Status { get; init; }

    /// <summary>Work order priority.</summary>
    public required string Priority { get; init; }

    /// <summary>UTC timestamp the work order was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>UTC timestamp the work order was last updated.</summary>
    public required DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>UTC timestamp the work order is due.</summary>
    public required DateTimeOffset DueAtUtc { get; init; }

    /// <summary>Optimistic-concurrency token for subsequent write commands.</summary>
    public required string ETag { get; init; }
}

/// <summary>Single task/step row within a work order.</summary>
public sealed record TaskStepDto
{
    /// <summary>Strictly-sequential step number.</summary>
    public required int StepNumber { get; init; }

    /// <summary>Task description.</summary>
    public required string Description { get; init; }

    /// <summary>Estimated duration in minutes.</summary>
    public required int DurationMinutes { get; init; }

    /// <summary>Task status.</summary>
    public required string Status { get; init; }

    /// <summary>Technician notes for this task, if any.</summary>
    public string? TechnicianNotes { get; init; }
}

/// <summary>Single part usage row within a work order (distinct from the Part inventory master).</summary>
public sealed record PartUsageDto
{
    /// <summary>Part identifier.</summary>
    public required Guid PartId { get; init; }

    /// <summary>Human-readable part number.</summary>
    public required string PartNumber { get; init; }

    /// <summary>Part display name.</summary>
    public required string PartName { get; init; }

    /// <summary>Quantity used.</summary>
    public required int QuantityUsed { get; init; }

    /// <summary>Whether the part was available at time of use.</summary>
    public required bool WasAvailable { get; init; }
}

/// <summary>Single chronological maintenance event row (work-order history or machine history).</summary>
public sealed record MaintenanceEventDto
{
    /// <summary>Event identifier.</summary>
    public required Guid EventId { get; init; }

    /// <summary>UTC timestamp the event occurred.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>Related work order code.</summary>
    public required string WorkOrderCode { get; init; }

    /// <summary>Technician display name.</summary>
    public required string TechnicianName { get; init; }

    /// <summary>Short summary of the event.</summary>
    public required string Summary { get; init; }

    /// <summary>Related work order identifier, for drilldown navigation.</summary>
    public required Guid WorkOrderId { get; init; }
}

/// <summary>
/// Preventive maintenance task row surfaced on the Maintenance module's "preventive schedule" page.
/// Reuse point: populated by delegating to the real Operational.PreventiveMaintenance module's
/// GetPreventiveMaintenanceQuery rather than a second, duplicated preventive-scheduling concept.
/// </summary>
public sealed record PreventiveTaskDto
{
    /// <summary>Preventive task identifier (the real PreventiveMaintenanceTask's identity).</summary>
    public required Guid TaskId { get; init; }

    /// <summary>Machine code this task targets (short code derived from EquipmentId; no machine master exists yet).</summary>
    public required string MachineCode { get; init; }

    /// <summary>Task description.</summary>
    public required string Description { get; init; }

    /// <summary>
    /// Trigger type (Time, Usage, Condition). Fixed to "Time" -- the real PreventiveMaintenanceTask
    /// aggregate does not yet carry a trigger-type field (assumption, documented here).
    /// </summary>
    public required string TriggerType { get; init; }

    /// <summary>Scheduled due date (UTC).</summary>
    public required DateTimeOffset DueAtUtc { get; init; }

    /// <summary>Whether the task is overdue.</summary>
    public required bool IsOverdue { get; init; }
}

/// <summary>Technician workload/assignment card, aggregated from open WorkOrder assignments.</summary>
public sealed record TechnicianWorkloadDto
{
    /// <summary>Technician identifier.</summary>
    public required Guid TechnicianId { get; init; }

    /// <summary>Technician display name.</summary>
    public required string TechnicianName { get; init; }

    /// <summary>
    /// Skill tags for this technician. Always empty -- no technician/skills master exists yet
    /// (assumption, documented here).
    /// </summary>
    public IReadOnlyList<string> SkillTags { get; init; } = [];

    /// <summary>Count of currently assigned (non-terminal) work orders.</summary>
    public required int AssignedCount { get; init; }

    /// <summary>
    /// Workload percentage (0-100+) relative to a fixed platform-default capacity of 8 concurrently
    /// assigned work orders (assumption, documented here and in the repository implementation --
    /// no per-technician capacity configuration exists yet).
    /// </summary>
    public required int WorkloadPercent { get; init; }

    /// <summary>Technician availability status (Available, Busy, Overloaded), derived from WorkloadPercent.</summary>
    public required string AvailabilityStatus { get; init; }
}

/// <summary>Single parts inventory row.</summary>
public sealed record PartInventoryDto
{
    /// <summary>Part identifier.</summary>
    public required Guid PartId { get; init; }

    /// <summary>Human-readable part number.</summary>
    public required string PartNumber { get; init; }

    /// <summary>Part display name.</summary>
    public required string PartName { get; init; }

    /// <summary>Quantity currently on hand.</summary>
    public required int QuantityOnHand { get; init; }

    /// <summary>Reorder threshold.</summary>
    public required int ReorderThreshold { get; init; }

    /// <summary>Storage location.</summary>
    public required string Location { get; init; }

    /// <summary>Whether the part is at or below its reorder threshold.</summary>
    public required bool NeedsReorder { get; init; }
}

/// <summary>
/// Single calendar event row for the Maintenance Calendar. New-build scope: this projection reads
/// only from the WorkOrder aggregate's DueAtUtc, keyed off a 4-hour nominal work window. A future
/// Scheduling-module rollup layer may enrich this with Preventive/Predictive-sourced events on top
/// of this same WorkOrder query surface (see ListWorkOrdersQuery's date-range filter).
/// </summary>
public sealed record MaintenanceCalendarEventDto
{
    /// <summary>Calendar event identifier (the underlying WorkOrderId).</summary>
    public required Guid EventId { get; init; }

    /// <summary>Related work order code, if any.</summary>
    public string? WorkOrderCode { get; init; }

    /// <summary>Target equipment display name (machine code).</summary>
    public required string EquipmentName { get; init; }

    /// <summary>Assigned technician display name, if any.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>Event start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Event end time (UTC).</summary>
    public required DateTimeOffset EndUtc { get; init; }

    /// <summary>Event type. Always "Maintenance" for WorkOrder-sourced rows.</summary>
    public required string EventType { get; init; }

    /// <summary>Event status (Planned, InProgress, Completed, Cancelled), mapped from WorkOrderStatus.</summary>
    public required string Status { get; init; }

    /// <summary>Event priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>
    /// Whether this event conflicts with another scheduled event for the same technician. Always
    /// <see langword="false"/> in this new-build scope -- conflict detection across multiple
    /// event sources is deferred to the future Scheduling-module rollup layer noted above.
    /// </summary>
    public required bool HasConflict { get; init; }
}

/// <summary>
/// Predictive maintenance insight for a work order/machine pair. Reuse point: populated by
/// delegating to the real Operational.PredictiveMaintenance module's
/// IPredictiveMaintenanceAlertRepository + PredictiveMaintenanceInsightsCalculator rather than a
/// second, duplicated prediction concept.
/// </summary>
public sealed record MaintenancePredictiveInsightsDto
{
    /// <summary>Normalized risk score (0-1).</summary>
    public required double RiskScore { get; init; }

    /// <summary>Ranked contributing factors.</summary>
    public required IReadOnlyList<string> ContributingFactors { get; init; }

    /// <summary>Recommended corrective action.</summary>
    public required string RecommendedAction { get; init; }

    /// <summary>Traceable model version identifier.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>UTC timestamp the insight was generated.</summary>
    public required DateTimeOffset GeneratedAtUtc { get; init; }
}

/// <summary>
/// Point-in-time (or current) WorkOrder state reconstructed purely by replaying its
/// <c>[System].[OutboxMessages]</c>-recorded domain-event history (Phase 6 §3.4 Replay/Rebuild
/// Across Systems). Mirrors the chronological-composition pattern established by
/// <c>PlantSafety.Domain.Auditing.AuditTimeline.Services.AuditTimelineCompositionService</c> for the
/// Phase 5 audit timeline, applied here to WorkOrder lifecycle rebuild.
/// </summary>
public sealed record WorkOrderRebuiltStateDto
{
    /// <summary>Identity of the rebuilt WorkOrder.</summary>
    public required Guid WorkOrderId { get; init; }

    /// <summary>UTC timestamp state was reconstructed as-of (the latest replayed event's OccurredAtUtc if no cutoff was supplied).</summary>
    public required DateTimeOffset AsOfUtc { get; init; }

    /// <summary>Reconstructed lifecycle status after replaying every event up to <see cref="AsOfUtc"/>.</summary>
    public required string Status { get; init; }

    /// <summary>Reconstructed creation timestamp, if a WorkOrderCreated event was replayed.</summary>
    public DateTimeOffset? CreatedAtUtc { get; init; }

    /// <summary>Last assigned technician identifier reconstructed from replayed events, if any.</summary>
    public Guid? LastAssignedTechnicianId { get; init; }

    /// <summary>Completion notes reconstructed from the replayed WorkOrderCompleted event, if any.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>The full ordered sequence of replayed events that produced this reconstructed state.</summary>
    public required IReadOnlyList<WorkOrderRebuiltTimelineEntryDto> ReplayedEvents { get; init; }
}

/// <summary>A single replayed event contributing to a <see cref="WorkOrderRebuiltStateDto"/>.</summary>
public sealed record WorkOrderRebuiltTimelineEntryDto
{
    /// <summary>The Outbox EventType discriminator (e.g. "WorkOrderCompleted").</summary>
    public required string EventType { get; init; }

    /// <summary>UTC timestamp the event occurred.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }
}
