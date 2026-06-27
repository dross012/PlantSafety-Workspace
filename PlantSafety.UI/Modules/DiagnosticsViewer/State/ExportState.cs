// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Export state container for the DiagnosticsViewer.
//   Tracks the active export format, export status, and export output.
//   The export flow is: UI → ExportDialog → ExportState → ResultExportManager
//   → Exporter → Formatter → Output. UI must NOT call formatters directly.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.State;

/// <summary>Supported export formats for diagnostic results.</summary>
public enum ExportFormat
{
    /// <summary>Comma-separated values.</summary>
    Csv,

    /// <summary>JavaScript Object Notation.</summary>
    Json,

    /// <summary>Markdown report.</summary>
    Markdown,

    /// <summary>Plain text summary.</summary>
    Text
}

/// <summary>Lifecycle states for an in-progress export operation.</summary>
public enum ExportStatus
{
    /// <summary>No export is in progress.</summary>
    Idle,

    /// <summary>Export is being prepared.</summary>
    Exporting,

    /// <summary>Export completed successfully.</summary>
    Completed,

    /// <summary>Export failed with a sanitized error.</summary>
    Failed
}

/// <summary>
/// Singleton state container for the DiagnosticsViewer export flow.
/// Tracks the chosen format and the lifecycle of the active export operation.
/// </summary>
public sealed class ExportState
{
    private ExportFormat _format = ExportFormat.Csv;
    private ExportStatus _status = ExportStatus.Idle;
    private string? _exportError;
    private DateTimeOffset? _completedAtUtc;

    /// <summary>Gets the currently selected export format.</summary>
    public ExportFormat Format => _format;

    /// <summary>Gets the current export operation status.</summary>
    public ExportStatus Status => _status;

    /// <summary>Gets a sanitized error message from a failed export, or null.</summary>
    public string? ExportError => _exportError;

    /// <summary>Gets the UTC time when the last successful export completed, or null.</summary>
    public DateTimeOffset? CompletedAtUtc => _completedAtUtc;

    /// <summary>Gets a value indicating whether an export is currently in progress.</summary>
    public bool IsExporting => _status == ExportStatus.Exporting;

    /// <summary>Raised when the export state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the desired export format.</summary>
    /// <param name="format">The target export format.</param>
    public void SetFormat(ExportFormat format)
    {
        _format = format;
        NotifyStateChanged();
    }

    /// <summary>Marks an export as started.</summary>
    public void BeginExport()
    {
        _status = ExportStatus.Exporting;
        _exportError = null;
        NotifyStateChanged();
    }

    /// <summary>Marks the export as successfully completed.</summary>
    public void MarkCompleted()
    {
        _status = ExportStatus.Completed;
        _completedAtUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Marks the export as failed with a sanitized error message.</summary>
    /// <param name="sanitizedError">A safe error message, free of PII and stack traces.</param>
    public void MarkFailed(string sanitizedError)
    {
        _status = ExportStatus.Failed;
        _exportError = sanitizedError;
        NotifyStateChanged();
    }

    /// <summary>Resets the export state to idle.</summary>
    public void Reset()
    {
        _status = ExportStatus.Idle;
        _exportError = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
