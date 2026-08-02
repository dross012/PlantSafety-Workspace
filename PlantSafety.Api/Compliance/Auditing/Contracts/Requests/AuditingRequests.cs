// ============================================================================
// File: AuditingRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Auditing
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Compliance.Auditing V1
//              controller. Covers the full lifecycle: Create -> Start -> RecordFindings -> Close
//              -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.Compliance.Auditing.Contracts.Requests;

/// <summary>Request body for submitting a new Auditing record.</summary>
public sealed record CreateAuditingRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable audit program/execution title.</summary>
    public required string AuditTitle { get; init; }

    /// <summary>Audit classification (Internal/External/Regulatory/Supplier).</summary>
    public string AuditType { get; init; } = "Internal";

    /// <summary>Narrative scope of the audit.</summary>
    public required string Scope { get; init; }

    /// <summary>Identity of the lead auditor assigned to this audit.</summary>
    public required Guid AuditorId { get; init; }

    /// <summary>Optional scheduled execution timestamp.</summary>
    public DateTime? ScheduledAtUtc { get; init; }
}

/// <summary>Request body for starting an audit program/execution (Created -&gt; Active).</summary>
public sealed record StartAuditingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Auditing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording an audit's findings and compliance score (Active -&gt; Completed).</summary>
public sealed record RecordAuditingFindingsRequest
{
    /// <summary>Count of findings raised during this audit.</summary>
    public required int FindingsCount { get; init; }

    /// <summary>Optional computed compliance score (0-100).</summary>
    public decimal? Score { get; init; }

    /// <summary>Overall compliance rating classification.</summary>
    public string ComplianceRating { get; init; } = "NotRated";

    /// <summary>Optimistic-concurrency token from the last read of this Auditing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed audit (Completed -&gt; Closed).</summary>
public sealed record CloseAuditingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Auditing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed audit for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveAuditingRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Auditing record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
