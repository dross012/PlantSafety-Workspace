// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Session-level state container for the DiagnosticsViewer
//   module. Holds the current result set, loading state, and session metadata.
//   All other DiagnosticsViewer state containers depend on this as the source
//   of truth for the active diagnostic result collection.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.DiagnosticsViewer.Models;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.State;

/// <summary>
/// Singleton state container for the active DiagnosticsViewer session.
/// Holds the loaded <see cref="DiagnosticResult"/> collection and tracks
/// loading and error state for the session.
/// </summary>
public sealed class DiagnosticsSessionState
{
    private IReadOnlyList<DiagnosticResult> _results = [];
    private bool _isLoading;
    private string? _loadError;
    private DateTimeOffset? _loadedAtUtc;
    private string _sessionLabel = string.Empty;

    /// <summary>Gets the current diagnostic result collection. Never null; empty when not loaded.</summary>
    public IReadOnlyList<DiagnosticResult> Results => _results;

    /// <summary>Gets a value indicating whether a load operation is in progress.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized error message from the last failed load, or null when no error.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the UTC timestamp when the current result set was loaded, or null when unloaded.</summary>
    public DateTimeOffset? LoadedAtUtc => _loadedAtUtc;

    /// <summary>Gets a human-readable label describing the source of the loaded results.</summary>
    public string SessionLabel => _sessionLabel;

    /// <summary>Gets a value indicating whether results are available.</summary>
    public bool HasResults => _results.Count > 0;

    /// <summary>Raised whenever the session state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the session as loading and clears any previous error.</summary>
    public void BeginLoad()
    {
        _isLoading = true;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Applies a freshly loaded result set and clears the loading flag.
    /// </summary>
    /// <param name="results">The diagnostic results returned by the API.</param>
    /// <param name="sessionLabel">A label describing the source (e.g., run ID or timestamp).</param>
    public void SetResults(IReadOnlyList<DiagnosticResult> results, string sessionLabel)
    {
        _results = results ?? [];
        _sessionLabel = sessionLabel ?? string.Empty;
        _isLoading = false;
        _loadError = null;
        _loadedAtUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>
    /// Records a load failure with a sanitized error message.
    /// </summary>
    /// <param name="sanitizedError">Safe error message, free of PII and stack traces.</param>
    public void SetLoadError(string sanitizedError)
    {
        _loadError = sanitizedError;
        _isLoading = false;
        NotifyStateChanged();
    }

    /// <summary>Clears all results, resetting the session to its initial state.</summary>
    public void Clear()
    {
        _results = [];
        _isLoading = false;
        _loadError = null;
        _loadedAtUtc = null;
        _sessionLabel = string.Empty;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
