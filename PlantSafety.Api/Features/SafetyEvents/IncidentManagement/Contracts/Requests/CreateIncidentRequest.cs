// ============================================================================
// File: CreateIncidentRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/incidents,
//              bound from the request body. Decouples the HTTP contract from the
//              Application-layer CreateIncidentCommand envelope fields.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Request body for creating a new Incident.</summary>
public sealed record CreateIncidentRequest
{
    /// <summary>Site where the incident occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short descriptive title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed narrative description. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the incident actually occurred.</summary>
    public required DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>Initial severity classification (e.g., Minor, Moderate, Serious, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Physical or logical location description. Max 500 characters.</summary>
    public required string Location { get; init; }

    /// <summary>Worker who is reporting the incident.</summary>
    public required Guid ReportedByWorkerId { get; init; }

    /// <summary>Worker assigned as investigator, if known at time of creation.</summary>
    public Guid? AssignedInvestigatorId { get; init; }
}
