// ============================================================================
// File: UpdateNearMissRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for PUT /features/safety-events/near-misses/{nearMissId},
//              bound from the request body.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.NearMisses.Contracts.Requests;

/// <summary>Request body for updating an existing Near Miss. Null fields mean no change.</summary>
public sealed record UpdateNearMissRequest
{
    /// <summary>Updated title. Null means no change.</summary>
    public string? Title { get; init; }

    /// <summary>Updated description. Null means no change.</summary>
    public string? Description { get; init; }

    /// <summary>Updated potential severity. Null means no change.</summary>
    public string? PotentialSeverity { get; init; }

    /// <summary>Updated hazard category. Null means no change.</summary>
    public string? HazardCategory { get; init; }

    /// <summary>Optimistic concurrency token from the last read of this Near Miss.</summary>
    public string? ETag { get; init; }
}
