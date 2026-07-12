// ============================================================================
// File: CreateNearMissRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/near-misses,
//              bound from the request body.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.NearMisses.Contracts.Requests;

/// <summary>Request body for creating a new Near Miss record.</summary>
public sealed record CreateNearMissRequest
{
    /// <summary>Site where the near miss occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Narrative description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the near miss occurred.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>Potential severity if the event had escalated.</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>Hazard category.</summary>
    public required string HazardCategory { get; init; }

    /// <summary>Worker who is reporting the near miss.</summary>
    public required Guid ReportedByWorkerId { get; init; }
}
