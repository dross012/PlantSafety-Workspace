// ============================================================================
// File: ListEmergencyResponsesRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing DTO for GET /features/safety-events/emergency-responses,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.EmergencyManagement.Contracts.Requests;

/// <summary>Query parameters for listing Emergency Response activations.</summary>
public sealed record ListEmergencyResponsesRequest
{
    /// <summary>1-based page number. Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by. Defaults to <c>ActivatedAtUtc</c>.</summary>
    public string OrderBy { get; init; } = "ActivatedAtUtc";

    /// <summary>Sort direction: <c>ASC</c> or <c>DESC</c>.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional status filter (e.g., Active, Standdown, Completed).</summary>
    public string? Status { get; init; }
}
