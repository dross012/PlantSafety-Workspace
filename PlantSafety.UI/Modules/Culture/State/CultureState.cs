// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Culture dashboard
//   module. Holds the currently loaded CultureIndex/Behavior/
//   EngagementScoring/TrendModeling snapshots for the active date range.
//   Components never call HttpClient directly — they dispatch to this state
//   via CultureApiClient, which is injected by the dashboard page.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Culture.Models;

namespace PlantSafety.UI.Modules.Culture.State;

/// <summary>Singleton state container for the Culture dashboard module.</summary>
public sealed class CultureState
{
    private IReadOnlyList<CultureIndexItemDto> _indexItems = [];
    private IReadOnlyList<BehaviorItemDto> _behaviorItems = [];
    private IReadOnlyList<EngagementScoringItemDto> _engagementItems = [];
    private IReadOnlyList<TrendModelingItemDto> _trendItems = [];
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the loaded culture index snapshots for the active date range. Never null.</summary>
    public IReadOnlyList<CultureIndexItemDto> IndexItems => _indexItems;

    /// <summary>Gets the loaded behavior observations for the active date range. Never null.</summary>
    public IReadOnlyList<BehaviorItemDto> BehaviorItems => _behaviorItems;

    /// <summary>Gets the loaded engagement scores for the active date range. Never null.</summary>
    public IReadOnlyList<EngagementScoringItemDto> EngagementItems => _engagementItems;

    /// <summary>Gets the loaded trend model outputs for the active date range. Never null.</summary>
    public IReadOnlyList<TrendModelingItemDto> TrendItems => _trendItems;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded dashboard data.</summary>
    public void SetData(
        IReadOnlyList<CultureIndexItemDto> indexItems,
        IReadOnlyList<BehaviorItemDto> behaviorItems,
        IReadOnlyList<EngagementScoringItemDto> engagementItems,
        IReadOnlyList<TrendModelingItemDto> trendItems)
    {
        _indexItems = indexItems;
        _behaviorItems = behaviorItems;
        _engagementItems = engagementItems;
        _trendItems = trendItems;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
