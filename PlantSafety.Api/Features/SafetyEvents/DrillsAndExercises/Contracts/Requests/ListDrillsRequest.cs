// ============================================================================
// File: ListDrillsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing DTO for GET /features/safety-events/drills,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Requests;

/// <summary>Query parameters for listing Safety Drills / Exercises.</summary>
public sealed record ListDrillsRequest
{
    /// <summary>1-based page number. Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by. Defaults to <c>ScheduledAtUtc</c>.</summary>
    public string OrderBy { get; init; } = "ScheduledAtUtc";

    /// <summary>Sort direction: <c>ASC</c> or <c>DESC</c>.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional status filter (e.g., Scheduled, Completed, Cancelled).</summary>
    public string? Status { get; init; }

    /// <summary>Optional drill type filter (e.g., Evacuation, FireResponse, ChemicalSpill, MedicalEmergency).</summary>
    public string? DrillType { get; init; }
}
