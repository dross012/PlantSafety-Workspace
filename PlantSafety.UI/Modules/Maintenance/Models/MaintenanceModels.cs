// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Maintenance module (Volume M1),
//   covering the Dashboard, Work Order Detail, Machine History, Preventive
//   Schedule, Technician Workload, Parts Inventory, and Predictive Insights
//   tab. Display-only — no domain logic. Additive-only evolution.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Maintenance.Models;

/// <summary>Work Order Summary card counts (Open/In Progress/Completed).</summary>
public sealed class WorkOrderSummaryDto
{
    /// <summary>Gets the count of open work orders.</summary>
    public int OpenCount { get; init; }

    /// <summary>Gets the count of in-progress work orders.</summary>
    public int InProgressCount { get; init; }

    /// <summary>Gets the count of completed work orders.</summary>
    public int CompletedCount { get; init; }
}

/// <summary>Machine Status Summary card counts (Healthy/Degraded/Offline).</summary>
public sealed class MachineStatusSummaryDto
{
    /// <summary>Gets the count of healthy machines.</summary>
    public int HealthyCount { get; init; }

    /// <summary>Gets the count of degraded machines.</summary>
    public int DegradedCount { get; init; }

    /// <summary>Gets the count of offline machines.</summary>
    public int OfflineCount { get; init; }
}

/// <summary>Single row in the Active Work Orders table.</summary>
public sealed class WorkOrderRowDto
{
    /// <summary>Gets the work order identifier (UUIDv7).</summary>
    public Guid WorkOrderId { get; init; }

    /// <summary>Gets the human-readable work order code (format WO-###).</summary>
    public string WorkOrderCode { get; init; } = string.Empty;

    /// <summary>Gets the machine identifier this work order targets.</summary>
    public Guid MachineId { get; init; }

    /// <summary>Gets the human-readable machine code (format M-###).</summary>
    public string MachineCode { get; init; } = string.Empty;

    /// <summary>Gets the assigned technician display name.</summary>
    public string TechnicianName { get; init; } = string.Empty;

    /// <summary>Gets the work order priority (Low, Medium, High, Critical).</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets the work order status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Full work order detail header.</summary>
public sealed class WorkOrderDetailDto
{
    /// <summary>Gets the work order identifier.</summary>
    public Guid WorkOrderId { get; init; }

    /// <summary>Gets the human-readable work order code.</summary>
    public string WorkOrderCode { get; init; } = string.Empty;

    /// <summary>Gets the target machine identifier.</summary>
    public Guid MachineId { get; init; }

    /// <summary>Gets the human-readable machine code.</summary>
    public string MachineCode { get; init; } = string.Empty;

    /// <summary>Gets the assigned technician display name.</summary>
    public string TechnicianName { get; init; } = string.Empty;

    /// <summary>Gets the work order status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the work order priority.</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the work order was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the work order was last updated.</summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the work order is due.</summary>
    public DateTimeOffset DueAtUtc { get; init; }
}

/// <summary>Single task/step row within a work order.</summary>
public sealed class TaskStepDto
{
    /// <summary>Gets the strictly-sequential step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>Gets the task description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the estimated duration in minutes.</summary>
    public int DurationMinutes { get; init; }

    /// <summary>Gets the task status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the technician notes for this task, if any.</summary>
    public string? TechnicianNotes { get; init; }
}

/// <summary>Single part usage row within a work order.</summary>
public sealed class PartUsageDto
{
    /// <summary>Gets the part identifier.</summary>
    public Guid PartId { get; init; }

    /// <summary>Gets the human-readable part number.</summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>Gets the part display name.</summary>
    public string PartName { get; init; } = string.Empty;

    /// <summary>Gets the quantity used.</summary>
    public int QuantityUsed { get; init; }

    /// <summary>Gets a value indicating whether the part was available at time of use.</summary>
    public bool WasAvailable { get; init; }
}

/// <summary>Single chronological maintenance event row.</summary>
public sealed class MaintenanceEventDto
{
    /// <summary>Gets the event identifier.</summary>
    public Guid EventId { get; init; }

    /// <summary>Gets the UTC timestamp the event occurred.</summary>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>Gets the related work order code.</summary>
    public string WorkOrderCode { get; init; } = string.Empty;

    /// <summary>Gets the technician display name.</summary>
    public string TechnicianName { get; init; } = string.Empty;

    /// <summary>Gets a short summary of the event.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Gets the related work order identifier, for drilldown navigation.</summary>
    public Guid WorkOrderId { get; init; }
}

/// <summary>Predictive maintenance insight for a work order/machine pair.</summary>
public sealed class MaintenancePredictiveInsightsDto
{
    /// <summary>Gets the normalized risk score (0-1).</summary>
    public double RiskScore { get; init; }

    /// <summary>Gets the ranked contributing factors.</summary>
    public IReadOnlyList<string> ContributingFactors { get; init; } = [];

    /// <summary>Gets the recommended corrective action.</summary>
    public string RecommendedAction { get; init; } = string.Empty;

    /// <summary>Gets the traceable model version identifier.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the insight was generated.</summary>
    public DateTimeOffset GeneratedAtUtc { get; init; }
}

/// <summary>Single preventive maintenance task on the calendar/list view.</summary>
public sealed class PreventiveTaskDto
{
    /// <summary>Gets the preventive task identifier.</summary>
    public Guid TaskId { get; init; }

    /// <summary>Gets the machine code this task targets.</summary>
    public string MachineCode { get; init; } = string.Empty;

    /// <summary>Gets the task description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the trigger type (Time, Usage, Condition).</summary>
    public string TriggerType { get; init; } = string.Empty;

    /// <summary>Gets the scheduled due date (UTC).</summary>
    public DateTimeOffset DueAtUtc { get; init; }

    /// <summary>Gets a value indicating whether the task is overdue.</summary>
    public bool IsOverdue { get; init; }
}

/// <summary>Technician workload/assignment card.</summary>
public sealed class TechnicianWorkloadDto
{
    /// <summary>Gets the technician identifier.</summary>
    public Guid TechnicianId { get; init; }

    /// <summary>Gets the technician display name.</summary>
    public string TechnicianName { get; init; } = string.Empty;

    /// <summary>Gets the skill tags for this technician.</summary>
    public IReadOnlyList<string> SkillTags { get; init; } = [];

    /// <summary>Gets the count of currently assigned work orders.</summary>
    public int AssignedCount { get; init; }

    /// <summary>Gets the workload percentage (0-100+) relative to capacity.</summary>
    public int WorkloadPercent { get; init; }

    /// <summary>Gets the technician availability status.</summary>
    public string AvailabilityStatus { get; init; } = string.Empty;
}

/// <summary>Single parts inventory row.</summary>
public sealed class PartInventoryDto
{
    /// <summary>Gets the part identifier.</summary>
    public Guid PartId { get; init; }

    /// <summary>Gets the human-readable part number.</summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>Gets the part display name.</summary>
    public string PartName { get; init; } = string.Empty;

    /// <summary>Gets the quantity currently on hand.</summary>
    public int QuantityOnHand { get; init; }

    /// <summary>Gets the reorder threshold.</summary>
    public int ReorderThreshold { get; init; }

    /// <summary>Gets the storage location.</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the part is below its reorder threshold.</summary>
    public bool NeedsReorder { get; init; }
}

/// <summary>Single calendar event row for the Maintenance Calendar (Volume MC1).</summary>
public sealed class MaintenanceCalendarEventDto
{
    /// <summary>Gets the calendar event identifier.</summary>
    public Guid EventId { get; init; }

    /// <summary>Gets the related work order code, if any.</summary>
    public string? WorkOrderCode { get; init; }

    /// <summary>Gets the target equipment display name.</summary>
    public string EquipmentName { get; init; } = string.Empty;

    /// <summary>Gets the assigned technician display name, if any.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>Gets the event start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the event end time (UTC).</summary>
    public DateTimeOffset EndUtc { get; init; }

    /// <summary>Gets the event type (Maintenance, Inspection, Predictive, Downtime, Other).</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Gets the event status (Planned, InProgress, Completed, Cancelled).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the event priority (Low, Medium, High, Critical).</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether this event conflicts with another scheduled event.</summary>
    public bool HasConflict { get; init; }
}
