// ============================================================================
// File: ListEnvironmentalHealthRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for GET /features/environmental-health/readings,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.EnvironmentalHealth.Readings.Contracts.Requests;

/// <summary>Query parameters for listing Environmental Health readings.</summary>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="OrderBy">Sort field (RecordedAtUtc, MetricType, Status).</param>
/// <param name="SortDirection">Sort direction.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="MetricType">Optional metric type filter.</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="From">Inclusive start of the query window (UTC).</param>
/// <param name="To">Inclusive end of the query window (UTC).</param>
public sealed record ListEnvironmentalHealthRequest(
    int       PageNumber = 1,
    int       PageSize = 25,
    string    OrderBy = "RecordedAtUtc",
    string    SortDirection = "DESC",
    Guid?     SiteId = null,
    string?   MetricType = null,
    string?   Status = null,
    DateTime? From = null,
    DateTime? To = null);
