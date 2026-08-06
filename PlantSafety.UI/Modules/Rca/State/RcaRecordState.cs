// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Singleton state container for the generic
//   ContinuousImprovement.RCA write/lifecycle surface (List/Search, Create,
//   Detail, lifecycle actions). Distinct from RcaState, which backs the
//   read-only Incident/NearMiss-linked RootCauseAnalysisDto pages.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Rca.Models;

namespace PlantSafety.UI.Modules.Rca.State;

/// <summary>Singleton state container for the ContinuousImprovement.RCA generic write surface.</summary>
public sealed class RcaRecordState
{
    private IReadOnlyList<RcaRecordItemDto> _items = [];
    private bool _isLoadingList;
    private string? _listError;

    private RcaRecordDetailDto? _selectedRecord;
    private bool _isLoadingRecord;
    private string? _recordError;
    private bool _isBusy;
    private string? _actionMessage;
    private string? _actionError;
    private bool _isCreating;
    private string? _createError;

    /// <summary>Gets the currently loaded list of RCA records for the active date range.</summary>
    public IReadOnlyList<RcaRecordItemDto> Items => _items;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets the currently loaded generic RCA record, or null.</summary>
    public RcaRecordDetailDto? SelectedRecord => _selectedRecord;

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

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of RCA records.</summary>
    public void SetItems(IReadOnlyList<RcaRecordItemDto> items) { _items = items; _isLoadingList = false; _listError = null; NotifyStateChanged(); }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the generic record as loading.</summary>
    public void BeginRecordLoad() { _isLoadingRecord = true; _recordError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded generic RCA record.</summary>
    public void SetSelectedRecord(RcaRecordDetailDto record) { _selectedRecord = record; _isLoadingRecord = false; _recordError = null; NotifyStateChanged(); }

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
