// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Singleton state container for the Operations.AuditViewer module.
//   Holds the current filter, search results, selected row (for the "Change Detail"
//   panel), and export/loading/error state for AuditViewerDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.OperationsAuditViewer.Models;

namespace PlantSafety.UI.Modules.OperationsAuditViewer.State;

/// <summary>Singleton state container for the Operations.AuditViewer module.</summary>
public sealed class AuditViewerState
{
    private AuditLogFilter _filter = new();
    private IReadOnlyList<AuditLogEntryDto> _items = [];
    private int _totalCount;
    private AuditLogEntryDto? _selectedEntry;
    private bool _isLoading;
    private bool _isExporting;
    private string? _error;
    private string? _exportError;

    /// <summary>Gets the active search filter.</summary>
    public AuditLogFilter Filter => _filter;

    /// <summary>Gets the current page of search results.</summary>
    public IReadOnlyList<AuditLogEntryDto> Items => _items;

    /// <summary>Gets the total number of rows matching the active filter, across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the entry currently selected for the "Change Detail" panel, if any.</summary>
    public AuditLogEntryDto? SelectedEntry => _selectedEntry;

    /// <summary>
    /// Gets the parsed "Key=Value" pairs from the selected entry's ErrorContext. This is NOT a
    /// structured before/after diff -- see AuditLogEntryDto's diff-view scope note.
    /// </summary>
    public IReadOnlyList<ChangeDetailField> SelectedChangeDetail => ParseErrorContext(_selectedEntry?.ErrorContext);

    /// <summary>Gets a value indicating whether a search is in flight.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether a CSV export is in flight.</summary>
    public bool IsExporting => _isExporting;

    /// <summary>Gets a sanitized search error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets a sanitized export error, or null.</summary>
    public string? ExportError => _exportError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Replaces the active filter (does not itself trigger a search).</summary>
    public void SetFilter(AuditLogFilter filter) { _filter = filter; NotifyStateChanged(); }

    /// <summary>Marks a search as in flight.</summary>
    public void BeginSearch() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of search results.</summary>
    public void SetResults(IReadOnlyList<AuditLogEntryDto> items, int totalCount)
    {
        _items = items;
        _totalCount = totalCount;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a search failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a row for the "Change Detail" panel. Pass null to close the panel.</summary>
    public void SelectEntry(AuditLogEntryDto? entry) { _selectedEntry = entry; NotifyStateChanged(); }

    /// <summary>Marks a CSV export as in flight.</summary>
    public void BeginExport() { _isExporting = true; _exportError = null; NotifyStateChanged(); }

    /// <summary>Marks a CSV export as completed.</summary>
    public void CompleteExport() { _isExporting = false; NotifyStateChanged(); }

    /// <summary>Records an export failure with a sanitized message.</summary>
    public void SetExportError(string sanitizedError) { _exportError = sanitizedError; _isExporting = false; NotifyStateChanged(); }

    /// <summary>
    /// Parses an ErrorContext string of "Key=Value; Key=Value" pairs into readable rows. Best
    /// effort: any segment without an '=' is surfaced verbatim under a "Detail" key so no
    /// content is silently dropped.
    /// </summary>
    private static IReadOnlyList<ChangeDetailField> ParseErrorContext(string? errorContext)
    {
        if (string.IsNullOrWhiteSpace(errorContext))
            return [];

        return errorContext
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment =>
            {
                var separatorIndex = segment.IndexOf('=');
                return separatorIndex > 0
                    ? new ChangeDetailField(segment[..separatorIndex].Trim(), segment[(separatorIndex + 1)..].Trim())
                    : new ChangeDetailField("Detail", segment);
            })
            .ToList();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
