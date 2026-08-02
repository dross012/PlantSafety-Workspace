// ============================================================================
// File: AiGovernanceRequests.cs
// Project: PlantSafety.Api
// Module: AiGovernance
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the AiGovernance V1 controller.
//              Covers the full lifecycle: Create -> Activate -> Complete -> Close -> Archive.
//              Mirrors PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs --
//              every mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.AiGovernance.Contracts.Requests;

/// <summary>Request body for registering a new AiGovernance record.</summary>
public sealed record CreateAiGovernanceRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the governed AI system.</summary>
    public required string SystemName { get; init; }

    /// <summary>Narrative description of the AI system's use case.</summary>
    public required string AiUseCase { get; init; }

    /// <summary>Risk classification (Low/Medium/High/Critical).</summary>
    public string RiskClassification { get; init; } = "Low";

    /// <summary>Identity of the accountable owner for this AI system.</summary>
    public required Guid OwnerUserId { get; init; }

    /// <summary>Whether this AI system requires human oversight before autonomous action.</summary>
    public bool RequiresHumanOversight { get; init; } = true;
}

/// <summary>Request body for activating an AiGovernance record (Created -&gt; Active).</summary>
public sealed record ActivateAiGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an AiGovernance record (Active -&gt; Completed).</summary>
public sealed record CompleteAiGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an AiGovernance record (Completed -&gt; Closed).</summary>
public sealed record CloseAiGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving an AiGovernance record (Closed -&gt; Archived).</summary>
public sealed record ArchiveAiGovernanceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
