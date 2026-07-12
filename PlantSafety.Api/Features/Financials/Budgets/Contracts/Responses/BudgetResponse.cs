// ============================================================================
// File: BudgetResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO for a single Safety Budget record.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.Budgets.Contracts.Responses;

/// <summary>Response payload for a single Safety Budget record.</summary>
public sealed record BudgetResponse(
    Guid           BudgetId,
    Guid           TenantId,
    Guid?          SiteId,
    int            FiscalYear,
    string         Category,
    decimal        AllocatedAmount,
    string         CurrencyCode,
    decimal        ActualSpend,
    decimal        RemainingBalance,
    string         Status,
    DateTimeOffset LastModifiedAtUtc,
    string         ETag);
