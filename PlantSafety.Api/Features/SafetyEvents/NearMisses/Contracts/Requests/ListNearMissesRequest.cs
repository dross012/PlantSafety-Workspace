// ============================================================================
// File: ListNearMissesRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for GET /features/safety-events/near-misses,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.NearMisses.Contracts.Requests;

/// <summary>Query parameters for listing Near Miss records.</summary>
public sealed record ListNearMissesRequest
{
    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by.</summary>
    public string OrderBy { get; init; } = "OccurredAtUtc";

    /// <summary>Sort direction: ASC or DESC.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional potential severity filter.</summary>
    public string? PotentialSeverity { get; init; }

    /// <summary>Optional hazard category filter.</summary>
    public string? HazardCategory { get; init; }

    /// <summary>Inclusive lower bound on occurrence date.</summary>
    public DateTimeOffset? OccurredFromUtc { get; init; }

    /// <summary>Inclusive upper bound on occurrence date.</summary>
    public DateTimeOffset? OccurredToUtc { get; init; }
}
