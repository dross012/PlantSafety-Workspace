// ============================================================================
// File: UpdateObservationRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for PUT /features/safety-events/observations/{observationId},
//              bound from the request body.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.Observation.Contracts.Requests;

/// <summary>Request body for updating an existing Safety Observation. Null fields mean no change.</summary>
public sealed record UpdateObservationRequest
{
    /// <summary>Updated title. Null means no change.</summary>
    public string? Title { get; init; }

    /// <summary>Updated description. Null means no change.</summary>
    public string? Description { get; init; }

    /// <summary>Updated observation type. Null means no change.</summary>
    public string? ObservationType { get; init; }

    /// <summary>Optimistic concurrency token from the last read of this Observation.</summary>
    public string? ETag { get; init; }
}
