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
