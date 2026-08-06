// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the Features/Financials/Budgets
//   CRUD-Query read endpoints (GET v1/features/financials/budgets,
//   GET v1/features/financials/budgets/{id}). Deserialized directly into the
//   inner Data shape of the ApiResponse&lt;T&gt; envelope, matching the
//   established sibling convention (see
//   Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Financials.Models;

/// <summary>A single Safety Budget record (GET v1/features/financials/budgets[/{id}]).</summary>
public sealed class BudgetDto
{
    /// <summary>Unique identifier.</summary>
    public Guid BudgetId { get; init; }

    /// <summary>Tenant owning this budget.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site this budget is allocated to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year this budget covers.</summary>
    public int FiscalYear { get; init; }

    /// <summary>Budget category (e.g., PPE, Training, Equipment, Compliance, Investigations).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Planned/allocated budget amount.</summary>
    public decimal AllocatedAmount { get; init; }

    /// <summary>Currency code (ISO 4217).</summary>
    public string CurrencyCode { get; init; } = string.Empty;

    /// <summary>Total actual spend to date against this budget.</summary>
    public decimal ActualSpend { get; init; }

    /// <summary>Remaining balance (AllocatedAmount - ActualSpend).</summary>
    public decimal RemainingBalance { get; init; }

    /// <summary>Workflow status (e.g., Draft, Active, Approved, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}

/// <summary>Paginated response payload for Safety Budget listings (GET v1/features/financials/budgets).</summary>
public sealed class BudgetListResponseDto
{
    /// <summary>Gets the matching budget items. Never null; empty when no results.</summary>
    public IReadOnlyList<BudgetDto> Items { get; init; } = [];

    /// <summary>Gets the total matching budget count across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the one-based current page number.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the maximum number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages { get; init; }

    /// <summary>Gets a value indicating whether there is a next page.</summary>
    public bool HasNextPage { get; init; }

    /// <summary>Gets a value indicating whether there is a previous page.</summary>
    public bool HasPreviousPage { get; init; }
}

/// <summary>
/// Request body for creating a new Safety Budget. Mirrors
/// PlantSafety.Api.Features.Financials.Budgets.Contracts.Requests.CreateBudgetRequest,
/// with DataAnnotations mirroring CreateBudgetCommandValidator's rules
/// (AllocatedAmount &gt; 0, FiscalYear in [2000, 2100], Category required).
/// Backed by POST v1/features/financials/budgets.
/// </summary>
public sealed class CreateBudgetRequest
{
    /// <summary>Site this budget is for. Null means enterprise-level.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Fiscal year (e.g., 2026).</summary>
    [Range(2000, 2100, ErrorMessage = "Fiscal year must be between 2000 and 2100.")]
    public int FiscalYear { get; set; } = DateTime.UtcNow.Year;

    /// <summary>Budget category.</summary>
    [Required(ErrorMessage = "Category is required.")]
    [StringLength(200, ErrorMessage = "Category must be 200 characters or fewer.")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Allocated amount. Must be positive.</summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Allocated amount must be greater than zero.")]
    public decimal AllocatedAmount { get; set; }

    /// <summary>Currency code (ISO 4217, e.g., USD).</summary>
    [Required(ErrorMessage = "Currency code is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be a 3-letter ISO 4217 code.")]
    public string CurrencyCode { get; set; } = "USD";
}

/// <summary>Response payload after creating a new Safety Budget (POST v1/features/financials/budgets).</summary>
public sealed class CreateBudgetResponse
{
    /// <summary>Gets the identifier of the newly created budget.</summary>
    public Guid BudgetId { get; init; }
}
