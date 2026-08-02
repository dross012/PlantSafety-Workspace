// ============================================================================
// File: ManagementOfChangeRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.ManagementOfChange
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Compliance.ManagementOfChange
//              V1 controller. Covers the full lifecycle: Create -> Approve -> Implement -> Close ->
//              Archive. Mirrors PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs
//              -- every mutating lifecycle request carries an optional ETag field so clients can
//              supply the optimistic-concurrency token captured from a prior read (e.g.
//              ManagementOfChangeItemDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Compliance.ManagementOfChange.Contracts.Requests;

/// <summary>Request body for submitting a new ManagementOfChange request.</summary>
public sealed record CreateManagementOfChangeRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable title of the proposed change.</summary>
    public required string ChangeTitle { get; init; }

    /// <summary>Change classification (Process, Equipment, Personnel, Procedure, Software).</summary>
    public string ChangeType { get; init; } = "Process";

    /// <summary>Business/safety justification for the change.</summary>
    public required string Justification { get; init; }

    /// <summary>Risk level of the change (Low, Medium, High, Critical).</summary>
    public string RiskLevel { get; init; } = "Low";
}

/// <summary>Request body for approving a ManagementOfChange request (Created -&gt; Active).</summary>
public sealed record ApproveManagementOfChangeRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording implementation of an approved change (Active -&gt; Completed).</summary>
public sealed record ImplementManagementOfChangeRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing an implemented change (Completed -&gt; Closed).</summary>
public sealed record CloseManagementOfChangeRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed change for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveManagementOfChangeRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
