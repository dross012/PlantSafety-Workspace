// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.Rollup
//  Layer:          Application
//  File:           SchedulingRollupModels.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-side DTOs backing the Scheduling UI's dashboard/job-detail/calendar
//                  pages (v1/scheduling/jobs*, v1/scheduling/resources/summary,
//                  v1/scheduling/calendar). Per docs/Phase5/ui-gaps-scoping.md's Scheduling
//                  section, "Job" is deliberately NOT a new domain aggregate -- it is a thin
//                  composition/read-model view over the real Operational.WorkOrders aggregate
//                  (the operational execution layer, matched almost 1:1 to the UI's "Job"
//                  concept) and the real Scheduling.PMScheduling sub-module (the planning-layer
//                  schedulable item closest in shape to a "Job"). Field names deliberately mirror
//                  PlantSafety.UI.Modules.Scheduling.Models.* 1:1 so the existing SchedulingApiClient
//                  and Razor pages need no changes. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.Rollup.Models;

/// <summary>Job Summary card counts (Pending/In Progress/Completed), rolled up across WorkOrders + PMScheduling.</summary>
public sealed record JobSummaryDto
{
    /// <summary>Count of pending jobs (WorkOrder Open + PMScheduling Created).</summary>
    public required int PendingCount { get; init; }

    /// <summary>Count of in-progress jobs (WorkOrder Assigned/InProgress/OnHold + PMScheduling Active).</summary>
    public required int InProgressCount { get; init; }

    /// <summary>Count of completed jobs (WorkOrder Completed + PMScheduling Completed).</summary>
    public required int CompletedCount { get; init; }
}

/// <summary>
/// Resource Summary card counts. Rolled up from Operational.WorkOrders' technician workload
/// query (the only real resource-availability read-model in the platform). IdleCount is always
/// 0 -- documented assumption: no technician/skills master roster exists yet, so technicians with
/// zero current assignments cannot be enumerated (only technicians who already have at least one
/// assignment appear in the source data).
/// </summary>
public sealed record ResourceSummaryDto
{
    /// <summary>Count of technicians with Available/Busy workload status.</summary>
    public required int ActiveCount { get; init; }

    /// <summary>Count of technicians with Overloaded workload status.</summary>
    public required int OverallocatedCount { get; init; }

    /// <summary>Always 0 -- see remarks on <see cref="ResourceSummaryDto"/>.</summary>
    public required int IdleCount { get; init; }
}

/// <summary>Single row in the Upcoming Jobs table. A "Job" is either a WorkOrder or a PMScheduling item.</summary>
public sealed record UpcomingJobDto
{
    /// <summary>Job identifier -- the underlying WorkOrderId or PMSchedulingId.</summary>
    public required Guid JobId { get; init; }

    /// <summary>Human-readable job code (the WorkOrderCode, or a synthesized "PM-######" code for PMScheduling-sourced jobs).</summary>
    public required string JobCode { get; init; }

    /// <summary>Assigned resource display name (technician name for WorkOrder jobs; equipment description for PMScheduling jobs, which carry no assigned technician).</summary>
    public required string ResourceName { get; init; }

    /// <summary>Scheduled start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Scheduled end time (UTC).</summary>
    public required DateTimeOffset EndUtc { get; init; }

    /// <summary>Normalized job status (Pending, InProgress, Completed, Cancelled).</summary>
    public required string Status { get; init; }
}

/// <summary>Full job detail (summary + metadata header). Sourced from WorkOrder when the identifier resolves there, else PMScheduling.</summary>
public sealed record JobDetailDto
{
    /// <summary>Job identifier (the underlying WorkOrderId or PMSchedulingId).</summary>
    public required Guid JobId { get; init; }

    /// <summary>Human-readable job code.</summary>
    public required string JobCode { get; init; }

    /// <summary>Job status code (raw source-module status).</summary>
    public required string Status { get; init; }

    /// <summary>Assigned resource display name.</summary>
    public required string ResourceName { get; init; }

    /// <summary>
    /// Resource identifier. For WorkOrder-sourced jobs, resolved by matching TechnicianName against
    /// the Operational.WorkOrders technician-workload read model (WorkOrderItemDto does not itself
    /// project a TechnicianId column); <see cref="Guid.Empty"/> when unassigned or unmatched. For
    /// PMScheduling-sourced jobs, this is the target EquipmentId (no distinct technician resource
    /// exists on a PMScheduling record).
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Scheduled duration in minutes. For WorkOrder jobs, the sum of its task steps' durations (240
    /// minutes -- a nominal 4-hour work window -- when no tasks are recorded). For PMScheduling jobs,
    /// always the same 240-minute nominal default (documented assumption -- PMScheduling records do
    /// not track a duration).
    /// </summary>
    public required int DurationMinutes { get; init; }

    /// <summary>UTC timestamp the job was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>UTC timestamp the job was last updated.</summary>
    public required DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>
    /// Display name of the operator who owns this job. For WorkOrder jobs, the same value as
    /// <see cref="ResourceName"/> (documented assumption: WorkOrder does not distinguish an
    /// "operator" role from the assigned technician). Empty for PMScheduling jobs (no operator
    /// concept exists on a PMScheduling record).
    /// </summary>
    public required string OperatorName { get; init; }
}

/// <summary>Single timeline step row within a job.</summary>
public sealed record TimelineStepDto
{
    /// <summary>Strictly-increasing step number.</summary>
    public required int StepNumber { get; init; }

    /// <summary>Step description.</summary>
    public required string Description { get; init; }

    /// <summary>Step start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Step end time (UTC).</summary>
    public required DateTimeOffset EndUtc { get; init; }

    /// <summary>Step status.</summary>
    public required string Status { get; init; }
}

/// <summary>
/// Single scheduling constraint row for a job. New-build scope, deliberately kept structural and
/// grounded in real WorkOrder fields only (Assignment/Schedule/Machine/Safety) rather than
/// reusing the Scheduling.PredictiveScheduling ConstraintEvaluator/IConstraintRule engine, which
/// requires SchedulableTask/ResourceAvailabilityWindow/ScheduleAssignment inputs this rollup layer
/// does not populate from real persisted data. Flagged for a follow-up epic if genuine
/// constraint-rule evaluation is required. Only produced for WorkOrder-sourced jobs; empty for
/// PMScheduling-sourced jobs (no constraint data exists on a PMScheduling record).
/// </summary>
public sealed record ConstraintDto
{
    /// <summary>Constraint type (Assignment, Schedule, Machine, Safety).</summary>
    public required string ConstraintType { get; init; }

    /// <summary>Human-readable description of the constraint.</summary>
    public required string Description { get; init; }

    /// <summary>Whether the constraint currently passes.</summary>
    public required bool IsSatisfied { get; init; }
}

/// <summary>
/// Predictive scheduling model output for a single job. Reuse point: for WorkOrder-sourced jobs,
/// delegates to Operational.WorkOrders' GetWorkOrderPredictiveInsightsQuery (itself a reuse of
/// Operational.PredictiveMaintenance); for PMScheduling-sourced jobs, invokes the same
/// Operational.PredictiveMaintenance alert repository/calculator directly against the PMScheduling
/// record's EquipmentId. RecommendedStartUtc/RecommendedEndUtc are computed in this rollup layer
/// (not stored anywhere): the due date, pulled forward 24h when RiskScore exceeds 0.6, plus a
/// nominal 4-hour work window.
/// </summary>
public sealed record PredictiveInsightsDto
{
    /// <summary>Normalized risk score (0-1).</summary>
    public required double RiskScore { get; init; }

    /// <summary>Model-recommended start time (UTC).</summary>
    public required DateTimeOffset RecommendedStartUtc { get; init; }

    /// <summary>Model-recommended end time (UTC).</summary>
    public required DateTimeOffset RecommendedEndUtc { get; init; }

    /// <summary>Traceable model version identifier.</summary>
    public required string ModelVersion { get; init; }
}

/// <summary>A single job chip placed on the calendar grid.</summary>
public sealed record CalendarJobDto
{
    /// <summary>Job identifier.</summary>
    public required Guid JobId { get; init; }

    /// <summary>Human-readable job code.</summary>
    public required string JobCode { get; init; }

    /// <summary>Assigned resource display name.</summary>
    public required string ResourceName { get; init; }

    /// <summary>Chip start time (UTC).</summary>
    public required DateTimeOffset StartUtc { get; init; }

    /// <summary>Chip end time (UTC).</summary>
    public required DateTimeOffset EndUtc { get; init; }

    /// <summary>Job status code.</summary>
    public required string Status { get; init; }
}

/// <summary>One calendar day column containing its job chips.</summary>
public sealed record CalendarDayDto
{
    /// <summary>Calendar date for this column.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Jobs scheduled on this day.</summary>
    public required IReadOnlyList<CalendarJobDto> Jobs { get; init; }
}

/// <summary>A full 7-day (Sun-Sat) calendar week, rolled up across WorkOrders + PMScheduling.</summary>
public sealed record CalendarWeekDto
{
    /// <summary>First date (Sunday) of the displayed week.</summary>
    public required DateOnly WeekStart { get; init; }

    /// <summary>The 7 day columns, Sunday through Saturday.</summary>
    public required IReadOnlyList<CalendarDayDto> Days { get; init; }
}
