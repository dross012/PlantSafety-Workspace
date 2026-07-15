// ============================================================================
// File: ListWorkerWellBeingRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for GET /human-factors/worker-wellbeing,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.WorkerWellBeing.Records.Contracts.Requests;

/// <summary>Query parameters for listing Worker WellBeing records.</summary>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="OrderBy">Sort field (RecordedAtUtc, WellBeingRiskLevel, Status).</param>
/// <param name="SortDirection">Sort direction.</param>
/// <param name="WorkerId">Optional worker filter.</param>
/// <param name="WellBeingRiskLevel">Optional risk level filter.</param>
/// <param name="Status">Optional status filter.</param>
public sealed record ListWorkerWellBeingRequest(
    int     PageNumber = 1,
    int     PageSize = 25,
    string  OrderBy = "RecordedAtUtc",
    string  SortDirection = "DESC",
    Guid?   WorkerId = null,
    string? WellBeingRiskLevel = null,
    string? Status = null);
