// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Singleton state container for the Operations.Monitoring module.
//   Holds DB Health, DB Size, DB Performance, DB Errors, and DB Deadlocks data plus
//   loading/error state for the MonitoringDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.OperationsMonitoring.Models;

namespace PlantSafety.UI.Modules.OperationsMonitoring.State;

/// <summary>Singleton state container for the Operations.Monitoring module.</summary>
public sealed class MonitoringState
{
    private DatabaseHealthReportDto? _health;
    private IReadOnlyList<DatabaseSizeSnapshotDto> _databaseSizes = [];
    private IReadOnlyList<QueryPerformanceItemDto> _queryPerformance = [];
    private IReadOnlyList<ErrorLogItemDto> _errors = [];
    private IReadOnlyList<DeadlockEventDto> _deadlocks = [];
    private string _selectedDatabaseKey = "CoreDB";
    private ErrorLogFilter _errorLogFilter = new();
    private ErrorLogItemDto? _selectedError;
    private bool _isLoading;
    private bool _isExportingErrors;
    private string? _error;
    private string? _exportError;

    /// <summary>Gets the aggregated /health/ready report, or null before the first load.</summary>
    public DatabaseHealthReportDto? Health => _health;

    /// <summary>Gets the recent database size snapshots.</summary>
    public IReadOnlyList<DatabaseSizeSnapshotDto> DatabaseSizes => _databaseSizes;

    /// <summary>Gets the top queries by average CPU time for the currently selected database.</summary>
    public IReadOnlyList<QueryPerformanceItemDto> QueryPerformance => _queryPerformance;

    /// <summary>Gets the recent rows from [PlantSafety.ErrorDB].dbo.ErrorLog.</summary>
    public IReadOnlyList<ErrorLogItemDto> Errors => _errors;

    /// <summary>Gets the recent deadlock events.</summary>
    public IReadOnlyList<DeadlockEventDto> Deadlocks => _deadlocks;

    /// <summary>Gets the currently selected database key for the DB Performance panel.</summary>
    public string SelectedDatabaseKey => _selectedDatabaseKey;

    /// <summary>Gets the active DB Errors search filter.</summary>
    public ErrorLogFilter ErrorLogFilter => _errorLogFilter;

    /// <summary>Gets the error row currently selected for the "Context Detail" panel, if any.</summary>
    public ErrorLogItemDto? SelectedError => _selectedError;

    /// <summary>
    /// Gets the parsed "Key=Value" pairs from the selected error's ErrorContext. This is a
    /// best-effort parse of caller-supplied free text -- ErrorLog has no CorrelationId column,
    /// so this is labeled "Context Detail", never "Correlation ID".
    /// </summary>
    public IReadOnlyList<ContextDetailField> SelectedContextDetail => ParseErrorContext(_selectedError?.ErrorContext);

    /// <summary>Gets a value indicating whether the dashboard data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether a DB Errors CSV export is in flight.</summary>
    public bool IsExportingErrors => _isExportingErrors;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets a sanitized DB Errors export error, or null.</summary>
    public string? ExportError => _exportError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Sets the database key used for the DB Performance panel.</summary>
    public void SetSelectedDatabaseKey(string databaseKey) { _selectedDatabaseKey = databaseKey; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded dashboard data.</summary>
    public void SetData(
        DatabaseHealthReportDto health,
        IReadOnlyList<DatabaseSizeSnapshotDto> databaseSizes,
        IReadOnlyList<QueryPerformanceItemDto> queryPerformance,
        IReadOnlyList<ErrorLogItemDto> errors,
        IReadOnlyList<DeadlockEventDto> deadlocks)
    {
        _health = health;
        _databaseSizes = databaseSizes;
        _queryPerformance = queryPerformance;
        _errors = errors;
        _deadlocks = deadlocks;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Replaces the active DB Errors search filter (does not itself trigger a reload).</summary>
    public void SetErrorLogFilter(ErrorLogFilter filter) { _errorLogFilter = filter; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of DB Errors results without touching the other panels.</summary>
    public void SetErrors(IReadOnlyList<ErrorLogItemDto> errors) { _errors = errors; NotifyStateChanged(); }

    /// <summary>Selects a row for the "Context Detail" panel. Pass null to close the panel.</summary>
    public void SelectError(ErrorLogItemDto? error) { _selectedError = error; NotifyStateChanged(); }

    /// <summary>Marks a DB Errors CSV export as in flight.</summary>
    public void BeginErrorExport() { _isExportingErrors = true; _exportError = null; NotifyStateChanged(); }

    /// <summary>Marks a DB Errors CSV export as completed.</summary>
    public void CompleteErrorExport() { _isExportingErrors = false; NotifyStateChanged(); }

    /// <summary>Records a DB Errors export failure with a sanitized message.</summary>
    public void SetExportError(string sanitizedError) { _exportError = sanitizedError; _isExportingErrors = false; NotifyStateChanged(); }

    /// <summary>
    /// Parses an ErrorContext string of "Key=Value; Key=Value" pairs into readable rows. Best
    /// effort: any segment without an '=' is surfaced verbatim under a "Detail" key so no
    /// content is silently dropped. NOTE: ErrorLog has no CorrelationId column -- this parses
    /// whatever caller-supplied context happens to be present, which may or may not include an
    /// identifier of any kind.
    /// </summary>
    private static IReadOnlyList<ContextDetailField> ParseErrorContext(string? errorContext)
    {
        if (string.IsNullOrWhiteSpace(errorContext))
            return [];

        return errorContext
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment =>
            {
                var separatorIndex = segment.IndexOf('=');
                return separatorIndex > 0
                    ? new ContextDetailField(segment[..separatorIndex].Trim(), segment[(separatorIndex + 1)..].Trim())
                    : new ContextDetailField("Detail", segment);
            })
            .ToList();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
