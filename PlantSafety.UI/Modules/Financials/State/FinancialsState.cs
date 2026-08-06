// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Financials module.
//   Holds the current paginated budget list (true server-side pagination —
//   the Features/Financials/Budgets endpoint supports it natively, unlike
//   the scaffold read-model endpoints used by the sibling Compliance/MOC/
//   Policies/RegulatoryCalendar/Checklist/Contractor modules), the selected
//   budget detail, and loading/error state. Also exposes dashboard KPI
//   aggregates computed from the currently loaded page, consumed by the
//   Financials dashboard's wired BudgetSummary/CostTable/ForecastChart
//   components.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Financials.Models;

namespace PlantSafety.UI.Modules.Financials.State;

/// <summary>Singleton state container for the Financials module.</summary>
public sealed class FinancialsState
{
    private IReadOnlyList<BudgetViewModel> _budgets = [];
    private BudgetViewModel? _selectedBudget;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    private bool _isCreatingBudget;
    private string? _createBudgetError;
    private Guid? _createdBudgetId;

    private bool _isRecordingCost;
    private string? _recordCostError;
    private Models.CreateFinancialRecordResult? _recordedFinancialRecord;

    /// <summary>Gets the current page of budget view models. Never null; empty when not loaded.</summary>
    public IReadOnlyList<BudgetViewModel> Budgets => _budgets;

    /// <summary>Gets the currently selected budget detail, or null when none is selected.</summary>
    public BudgetViewModel? SelectedBudget => _selectedBudget;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets the current page number (one-based).</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching budget count across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets a value indicating whether a Create Budget submission is in flight.</summary>
    public bool IsCreatingBudget => _isCreatingBudget;

    /// <summary>Gets a sanitized Create Budget submission error, or null.</summary>
    public string? CreateBudgetError => _createBudgetError;

    /// <summary>Gets the identifier of the most recently created budget, or null.</summary>
    public Guid? CreatedBudgetId => _createdBudgetId;

    /// <summary>Gets a value indicating whether a Record Cost (FinancialRecord create) submission is in flight.</summary>
    public bool IsRecordingCost => _isRecordingCost;

    /// <summary>Gets a sanitized Record Cost submission error, or null.</summary>
    public string? RecordCostError => _recordCostError;

    /// <summary>Gets the most recently recorded FinancialRecord result, or null.</summary>
    public Models.CreateFinancialRecordResult? RecordedFinancialRecord => _recordedFinancialRecord;

    /// <summary>Gets the sum of AllocatedAmount across the currently loaded page.</summary>
    public decimal TotalAllocated => _budgets.Sum(b => b.AllocatedAmount);

    /// <summary>Gets the sum of ActualSpend across the currently loaded page.</summary>
    public decimal TotalSpend => _budgets.Sum(b => b.ActualSpend);

    /// <summary>Gets the sum of RemainingBalance across the currently loaded page.</summary>
    public decimal TotalRemaining => _budgets.Sum(b => b.RemainingBalance);

    /// <summary>Gets the count of budgets currently over-allocated (spend exceeds allocation).</summary>
    public int OverBudgetCount => _budgets.Count(b => b.ActualSpend > b.AllocatedAmount);

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of budgets.</summary>
    public void SetBudgets(IReadOnlyList<BudgetViewModel> budgets, int pageNumber, int totalCount, int totalPages)
    {
        _budgets = budgets;
        _currentPage = pageNumber;
        _totalCount = totalCount;
        _totalPages = totalPages;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded budget detail record.</summary>
    public void SetSelectedBudget(BudgetViewModel budget) { _selectedBudget = budget; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected budget.</summary>
    public void ClearSelection() { _selectedBudget = null; NotifyStateChanged(); }

    /// <summary>Marks a Create Budget submission as in flight.</summary>
    public void BeginCreateBudget() { _isCreatingBudget = true; _createBudgetError = null; _createdBudgetId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Create Budget submission.</summary>
    public void SetCreateBudgetSuccess(Guid budgetId) { _createdBudgetId = budgetId; _isCreatingBudget = false; _createBudgetError = null; NotifyStateChanged(); }

    /// <summary>Records a Create Budget submission failure with a sanitized message.</summary>
    public void SetCreateBudgetError(string sanitizedError) { _createBudgetError = sanitizedError; _isCreatingBudget = false; NotifyStateChanged(); }

    /// <summary>Resets Create Budget submission state (e.g. when navigating to the form).</summary>
    public void ResetCreateBudget() { _isCreatingBudget = false; _createBudgetError = null; _createdBudgetId = null; NotifyStateChanged(); }

    /// <summary>Marks a Record Cost (FinancialRecord create) submission as in flight.</summary>
    public void BeginRecordCost() { _isRecordingCost = true; _recordCostError = null; _recordedFinancialRecord = null; NotifyStateChanged(); }

    /// <summary>Records a successful Record Cost submission.</summary>
    public void SetRecordCostSuccess(Models.CreateFinancialRecordResult result) { _recordedFinancialRecord = result; _isRecordingCost = false; _recordCostError = null; NotifyStateChanged(); }

    /// <summary>Records a Record Cost submission failure with a sanitized message.</summary>
    public void SetRecordCostError(string sanitizedError) { _recordCostError = sanitizedError; _isRecordingCost = false; NotifyStateChanged(); }

    /// <summary>Resets Record Cost submission state (e.g. when navigating to the form).</summary>
    public void ResetRecordCost() { _isRecordingCost = false; _recordCostError = null; _recordedFinancialRecord = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
