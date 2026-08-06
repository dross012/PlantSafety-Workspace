// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Certifications module.
//   Holds per-category (Contractor/DOT/EPA/IndustrySpecific/ISO14001/
//   ISO45001/OSHA) display-normalized row lists — already mapped via
//   CertificationsMapper — keyed by category, plus the selected certification
//   detail. Mirrors the tab-scoped lazy-load pattern established by
//   Modules/Admin/State/AdminSetupState.cs. Components never call HttpClient
//   directly — they dispatch to this state via CertificationsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Certifications.Models;

namespace PlantSafety.UI.Modules.Certifications.State;

/// <summary>Singleton state container for the Certifications module.</summary>
public sealed class CertificationsState
{
    private readonly Dictionary<string, IReadOnlyList<CertificationRowViewModel>> _rowsByCategory = new();
    private readonly Dictionary<string, bool> _isLoadingByCategory = new();
    private readonly Dictionary<string, string?> _errorByCategory = new();

    private CertificationRowViewModel? _selectedItem;
    private bool _isLoadingDetail;
    private string? _detailError;

    private CertificationItemDto? _selectedRecord;
    private bool _isLoadingRecord;
    private string? _recordError;
    private bool _isBusy;
    private string? _actionMessage;
    private string? _actionError;
    private bool _isCreating;
    private string? _createError;

    /// <summary>Gets the currently loaded rows for the given category key, or empty if not yet loaded.</summary>
    public IReadOnlyList<CertificationRowViewModel> RowsFor(string categoryKey) =>
        _rowsByCategory.TryGetValue(categoryKey, out var rows) ? rows : [];

    /// <summary>Gets a value indicating whether the given category is loading.</summary>
    public bool IsLoading(string categoryKey) => _isLoadingByCategory.TryGetValue(categoryKey, out var loading) && loading;

    /// <summary>Gets a value indicating whether the given category has ever been loaded.</summary>
    public bool IsLoaded(string categoryKey) => _rowsByCategory.ContainsKey(categoryKey);

    /// <summary>Gets the sanitized load error for the given category, or null.</summary>
    public string? ErrorFor(string categoryKey) => _errorByCategory.TryGetValue(categoryKey, out var err) ? err : null;

    /// <summary>Gets the currently selected certification detail, or null when none is selected.</summary>
    public CertificationRowViewModel? SelectedItem => _selectedItem;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the given category as loading.</summary>
    public void BeginCategoryLoad(string categoryKey)
    {
        _isLoadingByCategory[categoryKey] = true;
        _errorByCategory[categoryKey] = null;
        NotifyStateChanged();
    }

    /// <summary>Applies freshly loaded rows for the given category.</summary>
    public void SetCategoryRows(string categoryKey, IReadOnlyList<CertificationRowViewModel> rows)
    {
        _rowsByCategory[categoryKey] = rows;
        _isLoadingByCategory[categoryKey] = false;
        _errorByCategory[categoryKey] = null;
        NotifyStateChanged();
    }

    /// <summary>Records a category load failure with a sanitized message.</summary>
    public void SetCategoryError(string categoryKey, string sanitizedError)
    {
        _errorByCategory[categoryKey] = sanitizedError;
        _isLoadingByCategory[categoryKey] = false;
        NotifyStateChanged();
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded certification detail record.</summary>
    public void SetSelectedItem(CertificationRowViewModel item) { _selectedItem = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected certification.</summary>
    public void ClearSelection() { _selectedItem = null; NotifyStateChanged(); }

    /// <summary>Gets the currently loaded generic Certification write-surface record, or null.</summary>
    public CertificationItemDto? SelectedRecord => _selectedRecord;

    /// <summary>Gets a value indicating whether the generic record is loading.</summary>
    public bool IsLoadingRecord => _isLoadingRecord;

    /// <summary>Gets a sanitized generic record load error, or null.</summary>
    public string? RecordError => _recordError;

    /// <summary>Gets a value indicating whether a lifecycle action is in flight.</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Gets the last lifecycle action's informational message, or null.</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Gets the last lifecycle action's error message, or null.</summary>
    public string? ActionError => _actionError;

    /// <summary>Gets a value indicating whether a create operation is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets the last create operation's error message, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Marks the generic record as loading.</summary>
    public void BeginRecordLoad() { _isLoadingRecord = true; _recordError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded generic Certification record.</summary>
    public void SetSelectedRecord(CertificationItemDto record) { _selectedRecord = record; _isLoadingRecord = false; _recordError = null; NotifyStateChanged(); }

    /// <summary>Records a generic record load failure with a sanitized message.</summary>
    public void SetRecordError(string sanitizedError) { _recordError = sanitizedError; _isLoadingRecord = false; NotifyStateChanged(); }

    /// <summary>Marks a lifecycle action as in flight.</summary>
    public void BeginAction() { _isBusy = true; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a successful lifecycle action's informational message.</summary>
    public void CompleteAction(string message) { _isBusy = false; _actionMessage = message; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a failed lifecycle action with a sanitized message.</summary>
    public void SetActionError(string sanitizedError) { _isBusy = false; _actionError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Marks a create operation as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a failed create operation with a sanitized message.</summary>
    public void SetCreateError(string sanitizedError) { _isCreating = false; _createError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Resets create-form state.</summary>
    public void ResetCreate() { _isCreating = false; _createError = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
