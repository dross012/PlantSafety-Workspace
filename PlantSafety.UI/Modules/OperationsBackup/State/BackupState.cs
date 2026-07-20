// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-19
// FILE DESCRIPTION: Singleton state container for the Operations.Backup module.
//   Holds backup health, history, database size snapshots, and trigger/restore-verify
//   loading/error state for the BackupDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.OperationsBackup.Models;

namespace PlantSafety.UI.Modules.OperationsBackup.State;

/// <summary>Singleton state container for the Operations.Backup module.</summary>
public sealed class BackupState
{
    private IReadOnlyList<BackupHealthDto> _health = [];
    private IReadOnlyList<BackupHistoryItemDto> _history = [];
    private IReadOnlyList<DatabaseSizeSnapshotDto> _databaseSizes = [];
    private RestoreVerifyResultDto? _lastRestoreVerify;
    private bool _isLoading;
    private bool _isTriggering;
    private bool _isRestoreVerifying;
    private string? _error;
    private string? _triggerMessage;

    /// <summary>Gets the derived backup-health status per database.</summary>
    public IReadOnlyList<BackupHealthDto> Health => _health;

    /// <summary>Gets the recent backup history, newest first.</summary>
    public IReadOnlyList<BackupHistoryItemDto> History => _history;

    /// <summary>Gets the recent database size snapshots, newest first.</summary>
    public IReadOnlyList<DatabaseSizeSnapshotDto> DatabaseSizes => _databaseSizes;

    /// <summary>Gets the result of the most recently run restore-verify, if any.</summary>
    public RestoreVerifyResultDto? LastRestoreVerify => _lastRestoreVerify;

    /// <summary>Gets a value indicating whether the dashboard data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether a backup trigger request is in flight.</summary>
    public bool IsTriggering => _isTriggering;

    /// <summary>Gets a value indicating whether a restore-verify request is in flight.</summary>
    public bool IsRestoreVerifying => _isRestoreVerifying;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets a status message describing the outcome of the last trigger/restore-verify action.</summary>
    public string? TriggerMessage => _triggerMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded dashboard data.</summary>
    public void SetData(
        IReadOnlyList<BackupHealthDto> health,
        IReadOnlyList<BackupHistoryItemDto> history,
        IReadOnlyList<DatabaseSizeSnapshotDto> databaseSizes)
    {
        _health = health;
        _history = history;
        _databaseSizes = databaseSizes;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks a backup trigger as in flight.</summary>
    public void BeginTrigger() { _isTriggering = true; _triggerMessage = null; NotifyStateChanged(); }

    /// <summary>Records the outcome of a backup trigger request.</summary>
    public void SetTriggerMessage(string message) { _triggerMessage = message; _isTriggering = false; NotifyStateChanged(); }

    /// <summary>Marks a restore-verify as in flight.</summary>
    public void BeginRestoreVerify() { _isRestoreVerifying = true; NotifyStateChanged(); }

    /// <summary>Records the result of a restore-verify request.</summary>
    public void SetRestoreVerifyResult(RestoreVerifyResultDto result)
    {
        _lastRestoreVerify = result;
        _isRestoreVerifying = false;
        NotifyStateChanged();
    }

    /// <summary>Records a restore-verify failure with a sanitized message.</summary>
    public void SetRestoreVerifyError(string sanitizedError) { _triggerMessage = sanitizedError; _isRestoreVerifying = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
