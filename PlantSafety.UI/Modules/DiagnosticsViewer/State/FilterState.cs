// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Filter state container for the DiagnosticsViewer. Holds
//   the active severity, category, rule code, and target path filter criteria.
//   The DiagnosticFilterPipeline reads from this state to compute the filtered
//   result view without mutating the source DiagnosticsSessionState.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.DiagnosticsViewer.Models;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.State;

/// <summary>
/// Singleton filter state for the DiagnosticsViewer.
/// All filter mutations trigger OnChange so the results panel re-renders.
/// </summary>
public sealed class FilterState
{
    private IReadOnlySet<DiagnosticSeverity> _severities = new HashSet<DiagnosticSeverity>();
    private string _categoryFilter = string.Empty;
    private string _ruleCodeFilter = string.Empty;
    private string _targetFilter = string.Empty;
    private string _searchTerm = string.Empty;

    /// <summary>Gets the set of severity levels to include. Empty means all severities.</summary>
    public IReadOnlySet<DiagnosticSeverity> Severities => _severities;

    /// <summary>Gets the category substring filter. Empty means no category filter.</summary>
    public string CategoryFilter => _categoryFilter;

    /// <summary>Gets the rule code prefix filter. Empty means no rule filter.</summary>
    public string RuleCodeFilter => _ruleCodeFilter;

    /// <summary>Gets the target path substring filter. Empty means no target filter.</summary>
    public string TargetFilter => _targetFilter;

    /// <summary>Gets the free-text search term applied across message and rule code.</summary>
    public string SearchTerm => _searchTerm;

    /// <summary>Gets a value indicating whether any filter is currently active.</summary>
    public bool IsFiltered =>
        _severities.Count > 0 ||
        !string.IsNullOrWhiteSpace(_categoryFilter) ||
        !string.IsNullOrWhiteSpace(_ruleCodeFilter) ||
        !string.IsNullOrWhiteSpace(_targetFilter) ||
        !string.IsNullOrWhiteSpace(_searchTerm);

    /// <summary>Raised when any filter criterion changes.</summary>
    public event Action? OnChange;

    /// <summary>Applies a new set of severity filters.</summary>
    /// <param name="severities">The severity levels to include. Pass empty to remove severity filter.</param>
    public void SetSeverities(IReadOnlySet<DiagnosticSeverity> severities)
    {
        _severities = severities ?? new HashSet<DiagnosticSeverity>();
        NotifyStateChanged();
    }

    /// <summary>Sets the category substring filter.</summary>
    /// <param name="category">The category substring. Pass empty or null to clear.</param>
    public void SetCategoryFilter(string? category)
    {
        _categoryFilter = category?.Trim() ?? string.Empty;
        NotifyStateChanged();
    }

    /// <summary>Sets the rule code prefix filter.</summary>
    /// <param name="ruleCode">The rule code prefix. Pass empty or null to clear.</param>
    public void SetRuleCodeFilter(string? ruleCode)
    {
        _ruleCodeFilter = ruleCode?.Trim() ?? string.Empty;
        NotifyStateChanged();
    }

    /// <summary>Sets the target path substring filter.</summary>
    /// <param name="target">The target path substring. Pass empty or null to clear.</param>
    public void SetTargetFilter(string? target)
    {
        _targetFilter = target?.Trim() ?? string.Empty;
        NotifyStateChanged();
    }

    /// <summary>Sets the free-text search term.</summary>
    /// <param name="searchTerm">The search term. Pass empty or null to clear.</param>
    public void SetSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm?.Trim() ?? string.Empty;
        NotifyStateChanged();
    }

    /// <summary>Resets all filter criteria to their initial (unfiltered) state.</summary>
    public void ClearAll()
    {
        _severities = new HashSet<DiagnosticSeverity>();
        _categoryFilter = string.Empty;
        _ruleCodeFilter = string.Empty;
        _targetFilter = string.Empty;
        _searchTerm = string.Empty;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
