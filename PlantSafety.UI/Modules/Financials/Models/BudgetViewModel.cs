// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single Safety Budget,
//   used by both the Financials list/detail pages and the Financials
//   dashboard's wired BudgetSummary/CostTable/ForecastChart components.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Financials.Models;

/// <summary>List-row and detail view model for a single Safety Budget.</summary>
public sealed class BudgetViewModel
{
    /// <summary>Gets the budget identifier.</summary>
    public required Guid BudgetId { get; init; }

    /// <summary>Gets the fiscal year this budget covers.</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Gets the budget category.</summary>
    public required string Category { get; init; }

    /// <summary>Gets the allocated budget amount.</summary>
    public required decimal AllocatedAmount { get; init; }

    /// <summary>Gets the ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>Gets the actual spend to date.</summary>
    public required decimal ActualSpend { get; init; }

    /// <summary>Gets the remaining balance.</summary>
    public required decimal RemainingBalance { get; init; }

    /// <summary>Gets the percentage of the allocated amount spent so far, clamped to [0, 100] for display.</summary>
    public required decimal SpendPercentage { get; init; }

    /// <summary>Gets the raw workflow status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when enterprise-level.</summary>
    public Guid? SiteId { get; init; }
}
