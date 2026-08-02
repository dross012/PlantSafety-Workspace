// ============================================================================
// File: HumanFactorsRequests.cs
// Project: PlantSafety.Api
// Module: HumanFactors
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the HumanFactors V1
//              controller. Covers the full lifecycle: Create -> Activate -> Complete -> Close
//              -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs --
//              every mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.HumanFactors.Contracts.Requests;

/// <summary>Request body for submitting a new HumanFactors record.</summary>
public sealed record CreateHumanFactorsRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-factors classification (e.g. Fatigue/CognitiveLoad/WorkloadStress/Communication).</summary>
    public required string FactorType { get; init; }

    /// <summary>Identity of the worker affected by this assessment, if applicable.</summary>
    public Guid? AffectedUserId { get; init; }

    /// <summary>Severity rating on a 1-5 scale.</summary>
    public required int SeverityRating { get; init; }

    /// <summary>Narrative description of the assessed human factor.</summary>
    public required string Description { get; init; }
}

/// <summary>Request body for activating a HumanFactors record (Created -&gt; Active).</summary>
public sealed record ActivateHumanFactorsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HumanFactors record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing a HumanFactors record (Active -&gt; Completed).</summary>
public sealed record CompleteHumanFactorsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HumanFactors record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing a HumanFactors record (Completed -&gt; Closed).</summary>
public sealed record CloseHumanFactorsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HumanFactors record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed HumanFactors record (Closed -&gt; Archived).</summary>
public sealed record ArchiveHumanFactorsRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this HumanFactors record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
