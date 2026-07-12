// ============================================================================
// File: UpdateIncidentRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for PUT /features/safety-events/incidents/{incidentId},
//              bound from the request body.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Request body for updating an existing Incident. Null fields mean no change.</summary>
public sealed record UpdateIncidentRequest
{
    /// <summary>Updated title. Max 200 characters. Null means no change.</summary>
    public string? Title { get; init; }

    /// <summary>Updated description. Max 4000 characters. Null means no change.</summary>
    public string? Description { get; init; }

    /// <summary>Updated severity. Null means no change.</summary>
    public string? Severity { get; init; }

    /// <summary>Updated location. Null means no change.</summary>
    public string? Location { get; init; }

    /// <summary>Updated investigator assignment. Null means no change; empty Guid means unassign.</summary>
    public Guid? AssignedInvestigatorId { get; init; }

    /// <summary>Optimistic concurrency token from the last read of this Incident.</summary>
    public string? ETag { get; init; }
}
