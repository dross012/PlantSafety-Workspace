// ============================================================================
// File: PolicyRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.Policy
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Compliance.Policy V1
//              controller. Covers the full lifecycle: Create -> Approve -> RecordAcknowledgment
//              (repeatable) -> Supersede -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g. PolicyItemDto.ETag); a
//              stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.Contracts.Requests;

/// <summary>Request body for submitting a new Policy document.</summary>
public sealed record CreatePolicyRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable policy title.</summary>
    public required string PolicyTitle { get; init; }

    /// <summary>Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).</summary>
    public required string Category { get; init; }

    /// <summary>Whether personnel acknowledgment is required for this policy.</summary>
    public bool AcknowledgmentRequired { get; init; } = true;
}

/// <summary>Request body for approving a Policy and making it effective (Created -&gt; Active).</summary>
public sealed record ApprovePolicyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording an acknowledgment of an active Policy.</summary>
public sealed record RecordPolicyAcknowledgmentRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for superseding an active Policy with a newer revision (Active -&gt; Completed).</summary>
public sealed record SupersedePolicyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a superseded Policy (Completed -&gt; Closed).</summary>
public sealed record ClosePolicyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Policy for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchivePolicyRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
