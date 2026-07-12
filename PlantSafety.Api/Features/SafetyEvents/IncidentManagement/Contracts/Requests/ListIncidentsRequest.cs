// ============================================================================
// File: ListIncidentsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for GET /features/safety-events/incidents,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Query parameters for listing Incidents.</summary>
public sealed record ListIncidentsRequest
{
    /// <summary>1-based page number. Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by. Defaults to <c>IncidentOccurredAtUtc</c>.</summary>
    public string OrderBy { get; init; } = "IncidentOccurredAtUtc";

    /// <summary>Sort direction: <c>ASC</c> or <c>DESC</c>.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional status filter (e.g., Reported, UnderInvestigation).</summary>
    public string? Status { get; init; }

    /// <summary>Optional severity filter.</summary>
    public string? Severity { get; init; }

    /// <summary>Inclusive lower bound on occurrence date.</summary>
    public DateTimeOffset? OccurredFromUtc { get; init; }

    /// <summary>Inclusive upper bound on occurrence date.</summary>
    public DateTimeOffset? OccurredToUtc { get; init; }
}
