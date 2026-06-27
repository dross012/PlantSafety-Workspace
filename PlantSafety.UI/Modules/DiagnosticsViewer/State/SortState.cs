// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Sort state container for the DiagnosticsViewer. Canonical
//   ordering is Severity → Category → Rule → Target per Part XI-A. Sort
//   changes are applied by DiagnosticFilterPipeline as a deterministic pure
//   function — the state itself never mutates the result collection.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.State;

/// <summary>The field by which diagnostic results are sorted.</summary>
public enum DiagnosticSortField
{
    /// <summary>Sort by severity descending (Error first). This is the canonical default.</summary>
    Severity,

    /// <summary>Sort by category ascending.</summary>
    Category,

    /// <summary>Sort by rule code ascending.</summary>
    RuleCode,

    /// <summary>Sort by target file path ascending.</summary>
    Target,

    /// <summary>Sort by timestamp descending (newest first).</summary>
    Timestamp
}

/// <summary>
/// Singleton sort state for the DiagnosticsViewer.
/// The canonical default is <see cref="DiagnosticSortField.Severity"/> ascending
/// (i.e., Errors first, then Warnings, then Information).
/// </summary>
public sealed class SortState
{
    private DiagnosticSortField _sortField = DiagnosticSortField.Severity;
    private bool _descending;

    /// <summary>Gets the active sort field.</summary>
    public DiagnosticSortField SortField => _sortField;

    /// <summary>Gets a value indicating whether results are sorted in descending order.</summary>
    public bool IsDescending => _descending;

    /// <summary>Raised when the sort state changes.</summary>
    public event Action? OnChange;

    /// <summary>
    /// Sets the active sort field. Toggling the same field reverses the sort direction.
    /// Changing to a new field resets direction to the canonical default for that field.
    /// </summary>
    /// <param name="field">The field to sort by.</param>
    public void SetSort(DiagnosticSortField field)
    {
        if (_sortField == field)
        {
            _descending = !_descending;
        }
        else
        {
            _sortField = field;
            // Severity: Error=0 → ascending puts errors first (canonical default).
            // All other fields: ascending is the natural default.
            _descending = false;
        }

        NotifyStateChanged();
    }

    /// <summary>Resets sort to the canonical default (Severity ascending).</summary>
    public void Reset()
    {
        _sortField = DiagnosticSortField.Severity;
        _descending = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
