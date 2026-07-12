// ============================================================================
// File: CreateObservationRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/observations,
//              bound from the request body.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.Observation.Contracts.Requests;

/// <summary>Request body for creating a new Safety Observation.</summary>
public sealed record CreateObservationRequest
{
    /// <summary>Site where the observation was made.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Narrative description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Observation type: Positive, AtRisk, Unsafe.</summary>
    public required string ObservationType { get; init; }

    /// <summary>UTC timestamp when the observation was made.</summary>
    public required DateTimeOffset ObservedAtUtc { get; init; }

    /// <summary>Worker who made the observation.</summary>
    public required Guid ObservedByWorkerId { get; init; }
}
