// ============================================================================
// File: SustainabilityRequests.cs
// Project: PlantSafety.Api
// Module: Sustainability
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Sustainability V1
//              controller. Covers the full lifecycle: Create -> Activate -> Complete -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.Sustainability.Contracts.Requests;

/// <summary>Request body for submitting a new Sustainability initiative record.</summary>
public sealed record CreateSustainabilityRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable sustainability/ESG initiative name.</summary>
    public required string InitiativeName { get; init; }

    /// <summary>Initiative category (e.g., Emissions, WasteReduction, EnergyEfficiency, WaterConservation).</summary>
    public required string Category { get; init; }

    /// <summary>Optional target value for the initiative's tracked metric.</summary>
    public decimal? TargetValue { get; init; }

    /// <summary>Identity of the accountable initiative owner.</summary>
    public required Guid OwnerUserId { get; init; }
}

/// <summary>Request body for activating a sustainability initiative (Created -&gt; Active).</summary>
public sealed record ActivateSustainabilityRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Sustainability record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an active sustainability initiative (Active -&gt; Completed).</summary>
public sealed record CompleteSustainabilityRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Sustainability record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed sustainability initiative (Completed -&gt; Closed).</summary>
public sealed record CloseSustainabilityRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Sustainability record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed sustainability initiative (Closed -&gt; Archived).</summary>
public sealed record ArchiveSustainabilityRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this Sustainability record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
