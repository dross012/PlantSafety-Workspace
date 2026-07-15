// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Production Dashboard
//   page (Volume T1 §1). High-frequency-update capable (1-5s cycles per
//   Volume T1); updates only changed rows, never resets scroll/focus.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.ProductionTracking.Models;

namespace PlantSafety.UI.Modules.ProductionTracking.State;

/// <summary>Singleton state container for the Production Dashboard.</summary>
public sealed class ProductionDashboardState
{
    private ThroughputSummaryDto? _throughput;
    private BottleneckSummaryDto? _bottlenecks;
    private IReadOnlyList<ProductionRunRowDto> _activeRuns = [];
    private bool _isLoading;
    private string? _errorMessage;
    private DateTimeOffset _lastRefreshUtc;

    /// <summary>Gets the Throughput Summary card data, or null when not yet loaded.</summary>
    public ThroughputSummaryDto? Throughput => _throughput;

    /// <summary>Gets the Bottleneck Summary card data, or null when not yet loaded.</summary>
    public BottleneckSummaryDto? Bottlenecks => _bottlenecks;

    /// <summary>Gets the current active production runs.</summary>
    public IReadOnlyList<ProductionRunRowDto> ActiveRuns => _activeRuns;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the UTC timestamp of the last successful refresh.</summary>
    public DateTimeOffset LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a fully-refreshed dashboard snapshot (throughput, bottlenecks, active runs).</summary>
    public void SetSnapshot(ThroughputSummaryDto throughput, BottleneckSummaryDto bottlenecks, IReadOnlyList<ProductionRunRowDto> activeRuns)
    {
        _throughput = throughput;
        _bottlenecks = bottlenecks;
        _activeRuns = activeRuns;
        _isLoading = false;
        _errorMessage = null;
        _lastRefreshUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
