// ============================================================================
// File: ListProcessesRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for GET /features/process-management/processes,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Requests;

/// <summary>Query parameters for listing Process Definitions.</summary>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="OrderBy">Sort field (ProcessName, Category, Status).</param>
/// <param name="SortDirection">Sort direction.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="Category">Optional category filter.</param>
/// <param name="Status">Optional status filter.</param>
public sealed record ListProcessesRequest(
    int     PageNumber = 1,
    int     PageSize = 25,
    string  OrderBy = "ProcessName",
    string  SortDirection = "ASC",
    Guid?   SiteId = null,
    string? Category = null,
    string? Status = null);
