// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Comparison state container for the DiagnosticsViewer.
//   Holds a baseline result set alongside the primary session results so the
//   comparison view can highlight new, resolved, and changed findings.
//   Neither result set is mutated — comparison is computed by the pipeline.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.DiagnosticsViewer.Models;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.State;

/// <summary>Describes the comparison classification of a single diagnostic result.</summary>
public enum ComparisonClassification
{
    /// <summary>Present in the current run but not in the baseline (new finding).</summary>
    New,

    /// <summary>Present in the baseline but not in the current run (resolved finding).</summary>
    Resolved,

    /// <summary>Present in both runs with the same severity (unchanged).</summary>
    Unchanged,

    /// <summary>Present in both runs but with a changed severity.</summary>
    Changed
}

/// <summary>
/// Singleton state container for the DiagnosticsViewer comparison view.
/// Load a baseline result set to enable comparison against the active session.
/// </summary>
public sealed class ComparisonState
{
    private IReadOnlyList<DiagnosticResult> _baselineResults = [];
    private string _baselineLabel = string.Empty;
    private bool _isComparisonActive;

    /// <summary>Gets the baseline result set used for comparison. Empty when no comparison is active.</summary>
    public IReadOnlyList<DiagnosticResult> BaselineResults => _baselineResults;

    /// <summary>Gets the label describing the baseline session (e.g., run ID or date).</summary>
    public string BaselineLabel => _baselineLabel;

    /// <summary>Gets a value indicating whether a comparison is currently active.</summary>
    public bool IsComparisonActive => _isComparisonActive;

    /// <summary>Raised when the comparison state changes.</summary>
    public event Action? OnChange;

    /// <summary>
    /// Loads a baseline result set to enable comparison mode.
    /// </summary>
    /// <param name="baselineResults">The baseline diagnostic results.</param>
    /// <param name="baselineLabel">A label identifying the baseline run.</param>
    public void LoadBaseline(IReadOnlyList<DiagnosticResult> baselineResults, string baselineLabel)
    {
        _baselineResults = baselineResults ?? [];
        _baselineLabel = baselineLabel ?? string.Empty;
        _isComparisonActive = _baselineResults.Count > 0;
        NotifyStateChanged();
    }

    /// <summary>Clears the baseline and disables comparison mode.</summary>
    public void ClearBaseline()
    {
        _baselineResults = [];
        _baselineLabel = string.Empty;
        _isComparisonActive = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
