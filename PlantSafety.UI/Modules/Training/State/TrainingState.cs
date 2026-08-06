// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Training module. Holds
//   the currently loaded training record list (for the active date range)
//   and the selected training record detail. Components never call
//   HttpClient directly — they dispatch to this state via TrainingApiClient,
//   which is injected by the components that trigger loads.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Training.Models;

namespace PlantSafety.UI.Modules.Training.State;

/// <summary>Singleton state container for the Training module.</summary>
public sealed class TrainingState
{
    private IReadOnlyList<TrainingRecordItemDto> _items = [];
    private TrainingRecordItemDto? _selectedItem;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private bool _isAssigning;
    private string? _assignError;
    private Guid? _assignedTrainingRecordId;

    /// <summary>Gets the currently loaded training records for the active date range. Never null.</summary>
    public IReadOnlyList<TrainingRecordItemDto> Items => _items;

    /// <summary>Gets the currently selected training record detail, or null when none is selected.</summary>
    public TrainingRecordItemDto? SelectedItem => _selectedItem;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets a value indicating whether an Assign Training submission is in flight.</summary>
    public bool IsAssigning => _isAssigning;

    /// <summary>Gets a sanitized Assign Training submission error, or null.</summary>
    public string? AssignError => _assignError;

    /// <summary>Gets the identifier of the most recently assigned training record, or null.</summary>
    public Guid? AssignedTrainingRecordId => _assignedTrainingRecordId;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of training records.</summary>
    /// <param name="items">The items returned by the API for the requested date range.</param>
    public void SetItems(IReadOnlyList<TrainingRecordItemDto> items)
    {
        _items = items;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded training record detail.</summary>
    /// <param name="item">The full training record.</param>
    public void SetSelectedItem(TrainingRecordItemDto item) { _selectedItem = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected training record.</summary>
    public void ClearSelection() { _selectedItem = null; NotifyStateChanged(); }

    /// <summary>Marks an Assign Training submission as in flight.</summary>
    public void BeginAssign() { _isAssigning = true; _assignError = null; _assignedTrainingRecordId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Assign Training submission.</summary>
    public void SetAssignSuccess(Guid trainingRecordId) { _assignedTrainingRecordId = trainingRecordId; _isAssigning = false; _assignError = null; NotifyStateChanged(); }

    /// <summary>Records an Assign Training submission failure with a sanitized message.</summary>
    public void SetAssignError(string sanitizedError) { _assignError = sanitizedError; _isAssigning = false; NotifyStateChanged(); }

    /// <summary>Resets Assign Training submission state (e.g. when navigating to the form).</summary>
    public void ResetAssign() { _isAssigning = false; _assignError = null; _assignedTrainingRecordId = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
