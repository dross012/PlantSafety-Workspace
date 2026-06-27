// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Selected result state container for the DiagnosticsViewer.
//   Tracks which single DiagnosticResult (if any) is currently selected in the
//   results list for display in the detail panel. The selected result is never
//   mutated — the detail panel renders it read-only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;

using PlantSafety.UI.Modules.DiagnosticsViewer.Models;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.State;

/// <summary>
/// Singleton state container tracking the currently selected <see cref="DiagnosticResult"/>.
/// Null when no result is selected (initial state or after clearing selection).
/// </summary>
public sealed class SelectedResultState
{
    private DiagnosticResult? _selected;

    /// <summary>Gets the currently selected result, or null when nothing is selected.</summary>
    public DiagnosticResult? Selected => _selected;

    /// <summary>Gets a value indicating whether a result is currently selected.</summary>
    public bool HasSelection => _selected is not null;

    /// <summary>Raised when the selection changes.</summary>
    public event Action? OnChange;

    /// <summary>
    /// Selects the given diagnostic result. The result is not mutated.
    /// </summary>
    /// <param name="result">The result to select.</param>
    public void Select(DiagnosticResult result)
    {
        _selected = result ?? throw new ArgumentNullException(nameof(result));
        NotifyStateChanged();
    }

    /// <summary>Clears the current selection.</summary>
    public void Clear()
    {
        _selected = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
