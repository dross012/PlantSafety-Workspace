// ============================================================================
// File: WorkerWellBeingRequests.cs
// Project: PlantSafety.Api
// Module: WorkerWellBeing
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the WorkerWellBeing V1
//              controller. Covers the full lifecycle: Create -> Activate -> Complete -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.WorkerWellBeing.Contracts.Requests;

/// <summary>Request body for submitting a new WorkerWellBeing check-in record.</summary>
public sealed record CreateWorkerWellBeingRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Worker this check-in record relates to.</summary>
    public required Guid WorkerUserId { get; init; }

    /// <summary>Wellbeing risk classification (Low/Medium/High/Critical).</summary>
    public string WellBeingRiskLevel { get; init; } = "Low";

    /// <summary>Check-in classification (e.g., MentalHealth, PhysicalHealth, WorkLifeBalance).</summary>
    public required string CheckInType { get; init; }

    /// <summary>Self-reported or assessed wellbeing score, 1-10.</summary>
    public required int WellBeingScore { get; init; }

    /// <summary>Whether this check-in requires follow-up.</summary>
    public bool FollowUpRequired { get; init; }
}

/// <summary>Request body for activating a worker wellbeing check-in record (Created -&gt; Active).</summary>
public sealed record ActivateWorkerWellBeingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this WorkerWellBeing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an active worker wellbeing check-in record (Active -&gt; Completed).</summary>
public sealed record CompleteWorkerWellBeingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this WorkerWellBeing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed worker wellbeing check-in record (Completed -&gt; Closed).</summary>
public sealed record CloseWorkerWellBeingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this WorkerWellBeing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed worker wellbeing check-in record (Closed -&gt; Archived).</summary>
public sealed record ArchiveWorkerWellBeingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this WorkerWellBeing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
