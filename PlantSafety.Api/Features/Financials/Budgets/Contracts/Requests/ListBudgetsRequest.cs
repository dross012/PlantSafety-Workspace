// ============================================================================
// File: ListBudgetsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing request DTO for GET /features/financials/budgets,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.Budgets.Contracts.Requests;

/// <summary>Query parameters for listing Safety Budgets.</summary>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="OrderBy">Sort field (FiscalYear, AllocatedAmount, Status).</param>
/// <param name="SortDirection">Sort direction.</param>
/// <param name="FiscalYear">Optional fiscal year filter.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="Category">Optional category filter.</param>
/// <param name="Status">Optional status filter.</param>
public sealed record ListBudgetsRequest(
    int     PageNumber = 1,
    int     PageSize = 25,
    string  OrderBy = "FiscalYear",
    string  SortDirection = "DESC",
    int?    FiscalYear = null,
    Guid?   SiteId = null,
    string? Category = null,
    string? Status = null);
