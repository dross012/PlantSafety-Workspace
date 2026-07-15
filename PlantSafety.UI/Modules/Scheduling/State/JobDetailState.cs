// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Job Detail page
//   (Volume S1 §2). Holds the job summary/metadata, timeline, constraints,
//   predictive insights, and the active tab (Timeline|Resources|
//   Constraints|Predictive|Metadata).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Scheduling.Models;

namespace PlantSafety.UI.Modules.Scheduling.State;

/// <summary>Canonical Job Detail tab identifiers, in display order.</summary>
public enum JobDetailTab
{
    /// <summary>Timeline tab (default).</summary>
    Timeline = 0,

    /// <summary>Resources tab.</summary>
    Resources = 1,

    /// <summary>Constraints tab.</summary>
    Constraints = 2,

    /// <summary>Predictive insights tab.</summary>
    Predictive = 3,

    /// <summary>Metadata tab.</summary>
    Metadata = 4,
}

/// <summary>Singleton state container for the Job Detail page.</summary>
public sealed class JobDetailState
{
    private JobDetailDto? _job;
    private IReadOnlyList<TimelineStepDto> _timeline = [];
    private IReadOnlyList<ConstraintDto> _constraints = [];
    private PredictiveInsightsDto? _predictive;
    private JobDetailTab _activeTab = JobDetailTab.Timeline;
    private bool _isLoading;
    private bool _isLoadingTab;
    private string? _errorMessage;

    /// <summary>Gets the loaded job, or null when not yet loaded.</summary>
    public JobDetailDto? Job => _job;

    /// <summary>Gets the ordered timeline steps.</summary>
    public IReadOnlyList<TimelineStepDto> Timeline => _timeline;

    /// <summary>Gets the evaluated scheduling constraints.</summary>
    public IReadOnlyList<ConstraintDto> Constraints => _constraints;

    /// <summary>Gets the predictive insights for this job, or null when not yet loaded.</summary>
    public PredictiveInsightsDto? Predictive => _predictive;

    /// <summary>Gets the currently active tab.</summary>
    public JobDetailTab ActiveTab => _activeTab;

    /// <summary>Gets a value indicating whether the page header is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether the active tab's content is loading.</summary>
    public bool IsLoadingTab => _isLoadingTab;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page header as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded job header.</summary>
    public void SetJob(JobDetailDto job) { _job = job; _isLoading = false; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Marks the active tab as loading.</summary>
    public void BeginTabLoad() { _isLoadingTab = true; NotifyStateChanged(); }

    /// <summary>Applies loaded timeline steps.</summary>
    public void SetTimeline(IReadOnlyList<TimelineStepDto> timeline) { _timeline = timeline; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded constraints.</summary>
    public void SetConstraints(IReadOnlyList<ConstraintDto> constraints) { _constraints = constraints; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies freshly evaluated predictive insights.</summary>
    public void SetPredictive(PredictiveInsightsDto predictive) { _predictive = predictive; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(JobDetailTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; _isLoadingTab = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
