// ============================================================================
// File: SchedulingRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Scheduling V1 controller (PMScheduling, WorkOrderPriority,
//              EmergencyDrillPlanning, HazardIsolationSequencing, ShiftFatigue).
// ============================================================================
// UPDATED: 2026-07-30 -- EscalatePMSchedulingRequest/RecordPMSchedulingCompletionRequest/
//   EscalateWorkOrderPriorityRequest/ResolveWorkOrderPriorityRequest gain an optional ETag (Phase 4
//   Scheduling.PMScheduling/WorkOrderPriority concurrency remediation), mirroring
//   RiskManagementRequests.cs's identical precedent.

namespace PlantSafety.Api.Scheduling.Contracts.Requests;

/// <summary>Request body for scheduling a preventive maintenance task.</summary>
public sealed record SchedulePreventiveMaintenanceRequest
{
    /// <summary>UTC due date for the preventive maintenance task.</summary>
    public required DateTime DueAtUtc { get; init; }
}

/// <summary>Request body for recording completion of a preventive maintenance task.</summary>
public sealed record RecordPMSchedulingCompletionRequest
{
    /// <summary>Optional completion notes.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for escalating an overdue preventive maintenance task.</summary>
public sealed record EscalatePMSchedulingRequest
{
    /// <summary>Work Order Priority code assigned (P1-P4).</summary>
    public required string PriorityCode { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for escalating an overdue work order.</summary>
public sealed record EscalateWorkOrderPriorityRequest
{
    /// <summary>Reason for escalation.</summary>
    public required string EscalationReason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for resolving a work order priority record.</summary>
public sealed record ResolveWorkOrderPriorityRequest
{
    /// <summary>Optional resolution notes.</summary>
    public string? ResolutionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for scheduling a new emergency drill.</summary>
public sealed record ScheduleEmergencyDrillPlanningRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Drill classification (e.g., FireEvacuation, ChemicalSpill, SevereWeather, AlarmTest).</summary>
    public required string DrillType { get; init; }

    /// <summary>Shift the drill is scheduled for.</summary>
    public required string ShiftCode { get; init; }

    /// <summary>Optional UTC instant the drill is scheduled to occur.</summary>
    public DateTime? ScheduledAtUtc { get; init; }

    /// <summary>Expected/initial participation count.</summary>
    public int ParticipationCount { get; init; }
}

/// <summary>Request body for recording that a scheduled drill was conducted (Created -&gt; Active).</summary>
public sealed record ConductEmergencyDrillPlanningRequest
{
    /// <summary>Actual participation count recorded during the drill.</summary>
    public int ParticipationCount { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording the post-drill debrief/completion (Active -&gt; Completed).</summary>
public sealed record CompleteEmergencyDrillPlanningRequest
{
    /// <summary>Optional post-drill debrief notes.</summary>
    public string? DebriefNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a completed drill record (Completed -&gt; Closed).</summary>
public sealed record CloseEmergencyDrillPlanningRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed drill record for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveEmergencyDrillPlanningRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for a bulk Scheduling operation.</summary>
public sealed record BulkUpdateSchedulingRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifiers of the Scheduling resources to operate on. Maximum 500 per request.</summary>
    public required IReadOnlyList<Guid> ResourceIds { get; init; }

    /// <summary>The operation to apply (e.g. "Close", "Approve", "Reassign").</summary>
    public required string Operation { get; init; }

    /// <summary>Optional operation-specific key-value data.</summary>
    public Dictionary<string, string>? Parameters { get; init; }
}

/// <summary>Query-string bound filters for the Scheduling read-model list endpoints.</summary>
public sealed record SchedulingListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}

// ============================================================================
// Phase 4 full build-out (2026-07-30): HazardIsolationSequencing write-path request DTOs.
// Every mutating lifecycle request carries an optional ETag field so clients can supply the
// optimistic-concurrency token captured from a prior read (e.g.
// HazardIsolationSequencingItemDto.ETag); a stale value results in a concurrency-conflict error.
// Mirrors PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================

/// <summary>Request body for scheduling a new HazardIsolationSequencing record.</summary>
public sealed record ScheduleHazardIsolationSequencingRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Hazard zone the isolation window applies to.</summary>
    public required string HazardZone { get; init; }

    /// <summary>Classification of the hazardous task being isolated/sequenced.</summary>
    public required string TaskType { get; init; }

    /// <summary>Optional UTC start of the isolation window.</summary>
    public DateTime? WindowStartUtc { get; init; }

    /// <summary>Optional UTC end of the isolation window.</summary>
    public DateTime? WindowEndUtc { get; init; }
}

/// <summary>Request body for activating an isolation window (Created -&gt; Active).</summary>
public sealed record ActivateHazardIsolationSequencingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for flagging an incompatible-task conflict within an active isolation window.</summary>
public sealed record FlagHazardIsolationSequencingConflictRequest
{
    /// <summary>Reason the conflict is being flagged.</summary>
    public required string Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an isolation window (Active -&gt; Completed).</summary>
public sealed record CompleteHazardIsolationSequencingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed isolation record (Completed -&gt; Closed).</summary>
public sealed record CloseHazardIsolationSequencingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed isolation record for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveHazardIsolationSequencingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

// ============================================================================
// Phase 4 full build-out (2026-07-30): ShiftFatigue write-path request DTOs. Every mutating
// lifecycle request carries an optional ETag field so clients can supply the
// optimistic-concurrency token captured from a prior read (e.g. ShiftFatigueItemDto.ETag); a
// stale value results in a concurrency-conflict error. Mirrors
// PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs.
// ============================================================================

/// <summary>Request body for recording a new ShiftFatigue assessment.</summary>
public sealed record RecordShiftFatigueRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the employee the fatigue assessment applies to.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Shift pattern classification (e.g., Rotating, FixedNight, FourOnFourOff).</summary>
    public required string ShiftPattern { get; init; }

    /// <summary>Number of consecutive night shifts worked at the time of recording.</summary>
    public int ConsecutiveNightShifts { get; init; }

    /// <summary>Cumulative overtime hours at the time of recording.</summary>
    public decimal OvertimeHours { get; init; }

    /// <summary>Initial computed fatigue risk score.</summary>
    public decimal FatigueRiskScore { get; init; }
}

/// <summary>Request body for activating fatigue monitoring for an employee's shift record (Created -&gt; Active).</summary>
public sealed record ActivateShiftFatigueRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for reassessing the fatigue risk score during active monitoring.</summary>
public sealed record ReassessShiftFatigueRiskRequest
{
    /// <summary>Recomputed fatigue risk score.</summary>
    public required decimal FatigueRiskScore { get; init; }

    /// <summary>Updated consecutive-night-shifts count.</summary>
    public required int ConsecutiveNightShifts { get; init; }

    /// <summary>Updated cumulative overtime hours.</summary>
    public required decimal OvertimeHours { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing the fatigue monitoring period (Active -&gt; Completed).</summary>
public sealed record CompleteShiftFatigueRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed fatigue record (Completed -&gt; Closed).</summary>
public sealed record CloseShiftFatigueRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed fatigue record for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveShiftFatigueRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
