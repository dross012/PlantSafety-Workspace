// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting BudgetDto into the
//   display-ready BudgetViewModel shape. Named and shaped after the
//   platform's standard Mapster DTO -> ViewModel convention (Adapt-style
//   pure static projections, no side effects); implemented as hand-written
//   static methods rather than the Mapster package (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs for
//   the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Financials.Models;

namespace PlantSafety.UI.Modules.Financials.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Financials module.</summary>
public static class FinancialsMappingService
{
    /// <summary>Projects a Safety Budget DTO into a display-ready view model.</summary>
    public static BudgetViewModel ToViewModel(this BudgetDto dto) => new()
    {
        BudgetId = dto.BudgetId,
        FiscalYear = dto.FiscalYear,
        Category = dto.Category,
        AllocatedAmount = dto.AllocatedAmount,
        CurrencyCode = dto.CurrencyCode,
        ActualSpend = dto.ActualSpend,
        RemainingBalance = dto.RemainingBalance,
        SpendPercentage = dto.AllocatedAmount > 0
            ? System.Math.Clamp(dto.ActualSpend / dto.AllocatedAmount * 100m, 0m, 100m)
            : 0m,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        LastModifiedAtUtc = dto.LastModifiedAtUtc,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects a list of Safety Budget DTOs into display-ready view models.</summary>
    public static IReadOnlyList<BudgetViewModel> ToViewModels(this IEnumerable<BudgetDto> items) =>
        items.Select(ToViewModel).ToList();

    /// <summary>Maps a raw Budget workflow status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive: Draft/Active/Approved/Closed).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" or "approved" => "Healthy",
        "draft" or "submitted" or "pending" => "Warning",
        "overbudget" or "rejected" => "Critical",
        "closed" or "archived" => "Offline",
        _ => "Offline",
    };
}
