// ============================================================================
// File: BudgetListResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing paginated response DTO for Safety Budget listings.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.Budgets.Contracts.Responses;

/// <summary>Paginated response payload for Safety Budget listings.</summary>
public sealed record BudgetListResponse(
    IReadOnlyList<BudgetResponse> Items,
    int                           TotalCount,
    int                           PageNumber,
    int                           PageSize,
    int                           TotalPages,
    bool                          HasNextPage,
    bool                          HasPreviousPage);
