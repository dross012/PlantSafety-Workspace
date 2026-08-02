// ============================================================================
// File: PreventiveMaintenanceTaskRequests.cs
// Project: PlantSafety.Api
// Module: Operational.PreventiveMaintenance
// Created: 2026-07-30
// Description: Request DTOs bound from the HTTP request body for the PreventiveMaintenanceTask
//              write actions added to PreventiveMaintenanceV1Controller. Covers the full
//              PreventiveMaintenanceTask lifecycle: Schedule -> Start -> Complete -> Verify, plus
//              Cancel and MarkOverdue. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              PreventiveMaintenanceTaskItemDto.ETag); a stale value results in a
//              concurrency-conflict error. Distinct from the pre-existing
//              GetPreventiveMaintenanceRequest (date-range list query request, left untouched).
// ============================================================================

namespace PlantSafety.Api.Operational.PreventiveMaintenance.Contracts.Requests;

/// <summary>Request body for scheduling a new PreventiveMaintenanceTask.</summary>
public sealed record SchedulePreventiveMaintenanceTaskRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment scheduled for maintenance.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Description of the maintenance task.</summary>
    public required string TaskDescription { get; init; }

    /// <summary>UTC instant the task is scheduled for.</summary>
    public required DateTime ScheduledAtUtc { get; init; }
}

/// <summary>Request body for recording that maintenance work has begun (Scheduled -&gt; InProgress).</summary>
public sealed record StartPreventiveMaintenanceTaskRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording that maintenance work has been completed (InProgress -&gt; Completed).</summary>
public sealed record CompletePreventiveMaintenanceTaskRequest
{
    /// <summary>Free-text notes recorded at completion.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording independent verification of completed work (Completed -&gt; Verified).</summary>
public sealed record VerifyPreventiveMaintenanceTaskRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for cancelling a task before completion (Scheduled/InProgress -&gt; Cancelled).</summary>
public sealed record CancelPreventiveMaintenanceTaskRequest
{
    /// <summary>Reason for cancellation.</summary>
    public required string Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a scheduled task overdue (Scheduled -&gt; Overdue).</summary>
public sealed record MarkPreventiveMaintenanceTaskOverdueRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
