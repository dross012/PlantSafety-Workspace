// ============================================================================
// File: CreateBudgetRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing request DTO for POST /features/financials/budgets,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.Budgets.Contracts.Requests;

/// <summary>Request body for creating a new Safety Budget.</summary>
/// <param name="SiteId">Site this budget is for. Null means enterprise-level.</param>
/// <param name="FiscalYear">Fiscal year (e.g., 2026).</param>
/// <param name="Category">Budget category.</param>
/// <param name="AllocatedAmount">Allocated amount.</param>
/// <param name="CurrencyCode">Currency code (ISO 4217, e.g., USD).</param>
public sealed record CreateBudgetRequest(
    Guid?   SiteId,
    int     FiscalYear,
    string  Category,
    decimal AllocatedAmount,
    string  CurrencyCode);
