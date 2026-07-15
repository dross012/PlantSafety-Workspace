// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Production Run Detail
//   page (Volume T1 §2). Holds summary/metadata + Timeline/Batches/KPIs/
//   Events/Metadata tab content, lazy-loaded.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.ProductionTracking.Models;

namespace PlantSafety.UI.Modules.ProductionTracking.State;

/// <summary>Canonical Production Run Detail tab identifiers, in display order.</summary>
public enum ProductionRunDetailTab
{
    /// <summary>Timeline tab (default).</summary>
    Timeline = 0,

    /// <summary>Batches tab.</summary>
    Batches = 1,

    /// <summary>KPIs tab.</summary>
    Kpis = 2,

    /// <summary>Events tab.</summary>
    Events = 3,

    /// <summary>Metadata tab.</summary>
    Metadata = 4,
}

/// <summary>Singleton state container for the Production Run Detail page.</summary>
public sealed class ProductionRunDetailState
{
    private RunSummaryDto? _summary;
    private RunMetadataDto? _metadata;
    private IReadOnlyList<TimelineEventDto> _timeline = [];
    private IReadOnlyList<BatchRowDto> _batches = [];
    private IReadOnlyList<KpiTileDto> _kpis = [];
    private IReadOnlyList<EventRowDto> _events = [];
    private ProductionRunDetailTab _activeTab = ProductionRunDetailTab.Timeline;
    private bool _isLoading;
    private bool _isLoadingTab;
    private string? _errorMessage;

    /// <summary>Gets the loaded run summary, or null when not yet loaded.</summary>
    public RunSummaryDto? Summary => _summary;

    /// <summary>Gets the loaded run metadata, or null when not yet loaded.</summary>
    public RunMetadataDto? Metadata => _metadata;

    /// <summary>Gets the ordered timeline events.</summary>
    public IReadOnlyList<TimelineEventDto> Timeline => _timeline;

    /// <summary>Gets the batches for this run.</summary>
    public IReadOnlyList<BatchRowDto> Batches => _batches;

    /// <summary>Gets the KPI tiles for this run.</summary>
    public IReadOnlyList<KpiTileDto> Kpis => _kpis;

    /// <summary>Gets the event log for this run.</summary>
    public IReadOnlyList<EventRowDto> Events => _events;

    /// <summary>Gets the currently active tab.</summary>
    public ProductionRunDetailTab ActiveTab => _activeTab;

    /// <summary>Gets a value indicating whether the header is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether the active tab is loading.</summary>
    public bool IsLoadingTab => _isLoadingTab;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the header as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded run summary/metadata header.</summary>
    public void SetHeader(RunSummaryDto summary, RunMetadataDto metadata)
    {
        _summary = summary;
        _metadata = metadata;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Marks the active tab as loading.</summary>
    public void BeginTabLoad() { _isLoadingTab = true; NotifyStateChanged(); }

    /// <summary>Applies loaded timeline events, reordered chronologically.</summary>
    public void SetTimeline(IReadOnlyList<TimelineEventDto> timeline) { _timeline = [.. timeline.OrderBy(e => e.TimestampUtc)]; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded batches.</summary>
    public void SetBatches(IReadOnlyList<BatchRowDto> batches) { _batches = batches; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded KPI tiles.</summary>
    public void SetKpis(IReadOnlyList<KpiTileDto> kpis) { _kpis = kpis; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded events.</summary>
    public void SetEvents(IReadOnlyList<EventRowDto> events) { _events = events; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(ProductionRunDetailTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; _isLoadingTab = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
