// ============================================================================
// File: SchedulingRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Scheduling V1 controller (PMScheduling, WorkOrderPriority,
//              EmergencyDrillPlanning, HazardIsolationSequencing, ShiftFatigue).
// ============================================================================

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
}

/// <summary>Request body for escalating an overdue preventive maintenance task.</summary>
public sealed record EscalatePMSchedulingRequest
{
    /// <summary>Work Order Priority code assigned (P1-P4).</summary>
    public required string PriorityCode { get; init; }
}

/// <summary>Request body for escalating an overdue work order.</summary>
public sealed record EscalateWorkOrderPriorityRequest
{
    /// <summary>Reason for escalation.</summary>
    public required string EscalationReason { get; init; }
}

/// <summary>Request body for resolving a work order priority record.</summary>
public sealed record ResolveWorkOrderPriorityRequest
{
    /// <summary>Optional resolution notes.</summary>
    public string? ResolutionNotes { get; init; }
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
