// ============================================================================
// File: OperationalSafetyAggregateRequests.cs
// Project: PlantSafety.Api
// Module: OperationalSafety
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the OperationalSafety
//              aggregate-root V1 controller. Covers the full lifecycle:
//              Create -> Activate -> Complete -> Close -> Archive. Named distinctly
//              ("OperationalSafetyAggregateRequests", not "OperationalSafetyRequests") to avoid
//              any future collision with unrelated OperationalSafety.Alerts/Checklists/
//              PPEManagement/RiskManagement/Schedules request contracts. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs --
//              every mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.OperationalSafety.Contracts.Requests;

/// <summary>Request body for submitting a new OperationalSafety record.</summary>
public sealed record CreateOperationalSafetyRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Operational-safety domain (e.g. LineOfFire, WorkAtHeight, MobileEquipment).</summary>
    public required string SafetyDomain { get; init; }

    /// <summary>Risk level classification (Low/Medium/High/Critical).</summary>
    public string RiskLevel { get; init; } = "Low";

    /// <summary>Identity of the user accountable for this oversight record.</summary>
    public required Guid OwnerUserId { get; init; }

    /// <summary>UTC timestamp by which this record is next due for review, if scheduled.</summary>
    public DateTime? ReviewDueAtUtc { get; init; }
}

/// <summary>Request body for activating an OperationalSafety record (Created -&gt; Active).</summary>
public sealed record ActivateOperationalSafetyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this OperationalSafety record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an OperationalSafety record (Active -&gt; Completed).</summary>
public sealed record CompleteOperationalSafetyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this OperationalSafety record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an OperationalSafety record (Completed -&gt; Closed).</summary>
public sealed record CloseOperationalSafetyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this OperationalSafety record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed OperationalSafety record (Closed -&gt; Archived).</summary>
public sealed record ArchiveOperationalSafetyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this OperationalSafety record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
