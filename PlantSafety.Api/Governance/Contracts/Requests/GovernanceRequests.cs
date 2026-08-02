// ============================================================================
// File: GovernanceRequests.cs
// Project: PlantSafety.Api
// Module: Governance
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Governance V1 controller.
//              Covers the full lifecycle: Create -> Activate -> Complete -> Close -> Archive.
//              Mirrors PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs
//              -- every mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.Governance.Contracts.Requests;

/// <summary>Request body for submitting a new Governance record.</summary>
public sealed record CreateGovernanceRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Governance area (e.g. PolicyOversight, RiskGovernance, RegulatoryLiaison).</summary>
    public required string GovernanceArea { get; init; }

    /// <summary>Optional identifier of a linked policy record.</summary>
    public Guid? PolicyReferenceId { get; init; }

    /// <summary>Cadence, in months, at which this governance record must be reviewed. Defaults to 12.</summary>
    public int ReviewCycleMonths { get; init; } = 12;

    /// <summary>Identity of the user accountable for this governance record.</summary>
    public required Guid OwnerUserId { get; init; }
}

/// <summary>Request body for activating a Governance record (Created -&gt; Active).</summary>
public sealed record ActivateGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Governance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing a Governance record (Active -&gt; Completed).</summary>
public sealed record CompleteGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Governance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a Governance record (Completed -&gt; Closed).</summary>
public sealed record CloseGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Governance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed Governance record (Closed -&gt; Archived).</summary>
public sealed record ArchiveGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Governance record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
