// ============================================================================
// File: SearchIncidentsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for GET /features/safety-events/incidents/search,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Query parameters for full-text search across Incidents.</summary>
public sealed record SearchIncidentsRequest
{
    /// <summary>Text to search. Min 2, max 200 characters after normalization.</summary>
    public required string SearchTerm { get; init; }

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;
}
