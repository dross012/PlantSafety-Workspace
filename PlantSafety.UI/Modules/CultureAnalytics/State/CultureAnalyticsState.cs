// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Culture Analytics
//   page. Holds independent loading/error/data state for each of the four
//   data-backed tabs (Behavior, Culture Index, Engagement Scoring, Trend
//   Modeling) plus the shared date-range filter. Components never call
//   HttpClient directly — they dispatch to this state via
//   CultureAnalyticsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.CultureAnalytics.Models;

namespace PlantSafety.UI.Modules.CultureAnalytics.State;

/// <summary>The tabs presented on the Culture Analytics page.</summary>
public enum CultureAnalyticsTab
{
    /// <summary>Behavior observations tab.</summary>
    Behavior,

    /// <summary>Culture index tab.</summary>
    CultureIndex,

    /// <summary>Engagement scoring tab.</summary>
    EngagementScoring,

    /// <summary>Trend modeling tab.</summary>
    TrendModeling
}

/// <summary>Singleton state container for the Culture Analytics page.</summary>
public sealed class CultureAnalyticsState
{
    private CultureAnalyticsTab _activeTab = CultureAnalyticsTab.CultureIndex;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<BehaviorObservationDto> _behavior = [];
    private IReadOnlyList<CultureIndexSnapshotDto> _cultureIndex = [];
    private IReadOnlyList<EngagementScoreDto> _engagementScoring = [];
    private IReadOnlyList<CultureTrendModelDto> _trendModeling = [];

    private bool _isLoadingBehavior;
    private bool _isLoadingCultureIndex;
    private bool _isLoadingEngagementScoring;
    private bool _isLoadingTrendModeling;

    private string? _behaviorError;
    private string? _cultureIndexError;
    private string? _engagementScoringError;
    private string? _trendModelingError;

    /// <summary>Gets the currently active tab.</summary>
    public CultureAnalyticsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded behavior observations.</summary>
    public IReadOnlyList<BehaviorObservationDto> Behavior => _behavior;

    /// <summary>Gets the loaded culture index snapshots.</summary>
    public IReadOnlyList<CultureIndexSnapshotDto> CultureIndex => _cultureIndex;

    /// <summary>Gets the loaded engagement scores.</summary>
    public IReadOnlyList<EngagementScoreDto> EngagementScoring => _engagementScoring;

    /// <summary>Gets the loaded trend model outputs.</summary>
    public IReadOnlyList<CultureTrendModelDto> TrendModeling => _trendModeling;

    /// <summary>Gets a value indicating whether the Behavior tab is loading.</summary>
    public bool IsLoadingBehavior => _isLoadingBehavior;

    /// <summary>Gets a value indicating whether the Culture Index tab is loading.</summary>
    public bool IsLoadingCultureIndex => _isLoadingCultureIndex;

    /// <summary>Gets a value indicating whether the Engagement Scoring tab is loading.</summary>
    public bool IsLoadingEngagementScoring => _isLoadingEngagementScoring;

    /// <summary>Gets a value indicating whether the Trend Modeling tab is loading.</summary>
    public bool IsLoadingTrendModeling => _isLoadingTrendModeling;

    /// <summary>Gets a sanitized Behavior load error, or null.</summary>
    public string? BehaviorError => _behaviorError;

    /// <summary>Gets a sanitized Culture Index load error, or null.</summary>
    public string? CultureIndexError => _cultureIndexError;

    /// <summary>Gets a sanitized Engagement Scoring load error, or null.</summary>
    public string? EngagementScoringError => _engagementScoringError;

    /// <summary>Gets a sanitized Trend Modeling load error, or null.</summary>
    public string? TrendModelingError => _trendModelingError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(CultureAnalyticsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload the active tab afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _behavior = [];
        _cultureIndex = [];
        _engagementScoring = [];
        _trendModeling = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the Behavior tab as loading.</summary>
    public void BeginBehaviorLoad() { _isLoadingBehavior = true; _behaviorError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded behavior observations.</summary>
    public void SetBehavior(IReadOnlyList<BehaviorObservationDto> items) { _behavior = items; _isLoadingBehavior = false; _behaviorError = null; NotifyStateChanged(); }

    /// <summary>Records a Behavior load failure.</summary>
    public void SetBehaviorError(string sanitizedError) { _behaviorError = sanitizedError; _isLoadingBehavior = false; NotifyStateChanged(); }

    /// <summary>Marks the Culture Index tab as loading.</summary>
    public void BeginCultureIndexLoad() { _isLoadingCultureIndex = true; _cultureIndexError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded culture index snapshots.</summary>
    public void SetCultureIndex(IReadOnlyList<CultureIndexSnapshotDto> items) { _cultureIndex = items; _isLoadingCultureIndex = false; _cultureIndexError = null; NotifyStateChanged(); }

    /// <summary>Records a Culture Index load failure.</summary>
    public void SetCultureIndexError(string sanitizedError) { _cultureIndexError = sanitizedError; _isLoadingCultureIndex = false; NotifyStateChanged(); }

    /// <summary>Marks the Engagement Scoring tab as loading.</summary>
    public void BeginEngagementScoringLoad() { _isLoadingEngagementScoring = true; _engagementScoringError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded engagement scores.</summary>
    public void SetEngagementScoring(IReadOnlyList<EngagementScoreDto> items) { _engagementScoring = items; _isLoadingEngagementScoring = false; _engagementScoringError = null; NotifyStateChanged(); }

    /// <summary>Records an Engagement Scoring load failure.</summary>
    public void SetEngagementScoringError(string sanitizedError) { _engagementScoringError = sanitizedError; _isLoadingEngagementScoring = false; NotifyStateChanged(); }

    /// <summary>Marks the Trend Modeling tab as loading.</summary>
    public void BeginTrendModelingLoad() { _isLoadingTrendModeling = true; _trendModelingError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded trend model outputs.</summary>
    public void SetTrendModeling(IReadOnlyList<CultureTrendModelDto> items) { _trendModeling = items; _isLoadingTrendModeling = false; _trendModelingError = null; NotifyStateChanged(); }

    /// <summary>Records a Trend Modeling load failure.</summary>
    public void SetTrendModelingError(string sanitizedError) { _trendModelingError = sanitizedError; _isLoadingTrendModeling = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
