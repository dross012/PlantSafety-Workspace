// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the AI Training module. Holds
//   the loaded training-job window, the selected job for the detail drilldown,
//   and loading/error state. Components never call HttpClient directly — they
//   dispatch to this state via AiTrainingApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.AiTraining.Models;

namespace PlantSafety.UI.Modules.AiTraining.State;

/// <summary>Singleton state container for the AI Training module.</summary>
public sealed class AiTrainingState
{
    private IReadOnlyList<AiTrainingItemDto> _jobs = [];
    private AiTrainingItemDto? _selected;
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Gets the currently loaded training jobs. Never null; empty when not loaded.</summary>
    public IReadOnlyList<AiTrainingItemDto> Jobs => _jobs;

    /// <summary>Gets the currently selected training job for the detail view, or null.</summary>
    public AiTrainingItemDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the job list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the query window.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the inclusive end of the query window.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the query date range.</summary>
    public void SetRange(DateOnly start, DateOnly end) { _rangeStart = start; _rangeEnd = end; NotifyStateChanged(); }

    /// <summary>Marks the job list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of training jobs.</summary>
    /// <param name="jobs">The job items returned by the API.</param>
    public void SetJobs(IReadOnlyList<AiTrainingItemDto> jobs)
    {
        _jobs = jobs;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a job by identifier from the currently loaded set, if present.</summary>
    /// <param name="trainingJobId">The training job identifier to select.</param>
    /// <returns>True if the job was found and selected.</returns>
    public bool SelectById(Guid trainingJobId)
    {
        _selected = _jobs.FirstOrDefault(j => j.TrainingJobId == trainingJobId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected job.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
