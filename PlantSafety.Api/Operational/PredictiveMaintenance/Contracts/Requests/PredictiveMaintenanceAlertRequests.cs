// ============================================================================
// File: PredictiveMaintenanceAlertRequests.cs
// Project: PlantSafety.Api
// Module: Operational.PredictiveMaintenance
// Created: 2026-07-30
// Description: Request DTOs bound from the HTTP request body for the
//              Operational.PredictiveMaintenance Alert V1 controller. Covers the full
//              PredictiveMaintenanceAlert lifecycle: Raise -> Review -> ScheduleAction ->
//              Resolve/Dismiss -> Close. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              PredictiveMaintenanceAlertItemDto.ETag); a stale value results in a
//              concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Operational.PredictiveMaintenance.Contracts.Requests;

/// <summary>Request body for raising a new PredictiveMaintenanceAlert.</summary>
public sealed record RaisePredictiveMaintenanceAlertRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment being monitored.</summary>
    public required Guid EquipmentId { get; init; }

    /// <summary>Predicted failure mode/type.</summary>
    public required string PredictedFailureType { get; init; }

    /// <summary>UTC instant the failure was predicted.</summary>
    public required DateTime PredictedAtUtc { get; init; }
}

/// <summary>Request body for recording a reviewer's triage of an alert (Raised -&gt; UnderReview).</summary>
public sealed record ReviewPredictiveMaintenanceAlertRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this alert. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for scheduling corrective maintenance action (UnderReview -&gt; ActionScheduled).</summary>
public sealed record SchedulePredictiveMaintenanceAlertActionRequest
{
    /// <summary>Optional reference to the work order/maintenance ticket raised for this alert.</summary>
    public string? WorkOrderReference { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this alert. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording that a predicted failure has been addressed (ActionScheduled -&gt; Resolved).</summary>
public sealed record ResolvePredictiveMaintenanceAlertRequest
{
    /// <summary>Free-text resolution notes.</summary>
    public string? ResolutionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this alert. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for dismissing an alert without corrective action (Raised/UnderReview -&gt; Dismissed).</summary>
public sealed record DismissPredictiveMaintenanceAlertRequest
{
    /// <summary>Reason for dismissal.</summary>
    public required string Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this alert. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a resolved or dismissed alert (Resolved/Dismissed -&gt; Closed).</summary>
public sealed record ClosePredictiveMaintenanceAlertRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this alert. Null skips the check.</summary>
    public string? ETag { get; init; }
}
