// ============================================================================
// File: ProcessDefinitionRequests.cs
// Project: PlantSafety.Api
// Module: ProcessManagement
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the ProcessManagement V1
//              controller. Covers the full lifecycle: Create -> SubmitForReview -> Approve ->
//              Retire -> Archive, plus the same-status Revise action. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.ProcessManagement.Contracts.Requests;

/// <summary>Request body for submitting a new ProcessDefinition record.</summary>
public sealed record CreateProcessDefinitionRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable process name.</summary>
    public required string ProcessName { get; init; }

    /// <summary>Safety-critical process category.</summary>
    public required string Category { get; init; }

    /// <summary>Identity of the accountable process owner.</summary>
    public required Guid OwnerId { get; init; }
}

/// <summary>Request body for submitting a process definition for review (Draft -&gt; UnderReview).</summary>
public sealed record SubmitProcessDefinitionForReviewRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ProcessDefinition record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for approving a process definition under review (UnderReview -&gt; Active).</summary>
public sealed record ApproveProcessDefinitionRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ProcessDefinition record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for retiring an active process definition (Active -&gt; Retired).</summary>
public sealed record RetireProcessDefinitionRequest
{
    /// <summary>Optional narrative reason for retirement.</summary>
    public string? Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this ProcessDefinition record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a retired process definition (Retired -&gt; Archived).</summary>
public sealed record ArchiveProcessDefinitionRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ProcessDefinition record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for revising an active process definition in place (same-status action; increments Version).</summary>
public sealed record ReviseProcessDefinitionRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this ProcessDefinition record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
