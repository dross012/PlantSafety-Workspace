// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-19
// FILE DESCRIPTION: Client-side DTOs for the Operations.Backup module. Mirror
//   PlantSafety.Application.Operations.Backup.Models.* returned by
//   PlantSafety.Api.Operations.Backup.Controllers.V1.OperationsBackupV1Controller.
//   Surfaces the real SQL Server backup/DR automation built in Phase 12 + Phase 18
//   (msdb.dbo.usp_PlantSafety_Backup*, usp_PlantSafety_RestoreTest,
//   PlantSafety_DatabaseSizeLog) -- NOT the unrelated CrossDomain.Backup CRUD stub.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.OperationsBackup.Models;

/// <summary>Backup type selector mirroring Invoke-PlantSafetyBackup.ps1 -BackupType.</summary>
public enum BackupType
{
    /// <summary>Full backup. Weekly cadence.</summary>
    Full = 0,

    /// <summary>Differential backup. Daily cadence.</summary>
    Diff = 1,

    /// <summary>Transaction log backup (only applies to FULL/BULK_LOGGED recovery model databases).</summary>
    Log = 2
}

/// <summary>Acknowledgement returned immediately after a backup process is launched.</summary>
public sealed class BackupTriggerResultDto
{
    /// <summary>Gets the backup type that was triggered.</summary>
    public BackupType BackupType { get; init; }

    /// <summary>Gets the OS process id of the launched backup host process, if available.</summary>
    public int? ProcessId { get; init; }

    /// <summary>Gets the UTC timestamp the trigger request was accepted.</summary>
    public DateTime StartedUtc { get; init; }

    /// <summary>Gets the acknowledgement status (always "Started" on success).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the directory the backup task log is written to.</summary>
    public string LogDirectory { get; init; } = string.Empty;
}

/// <summary>A single completed backup, sourced from msdb.dbo.backupset.</summary>
public sealed class BackupHistoryItemDto
{
    /// <summary>Gets the PlantSafety.* database the backup was taken against.</summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Gets the human-readable backup type ("Full", "Differential", "Transaction Log").</summary>
    public string BackupTypeDescription { get; init; } = string.Empty;

    /// <summary>Gets when the backup completed.</summary>
    public DateTime BackupFinishDateUtc { get; init; }

    /// <summary>Gets the backup set size in megabytes.</summary>
    public decimal SizeMB { get; init; }

    /// <summary>Gets the full path to the .bak/.trn file on disk.</summary>
    public string PhysicalDeviceName { get; init; } = string.Empty;
}

/// <summary>A single file-level database size snapshot from msdb.dbo.PlantSafety_DatabaseSizeLog.</summary>
public sealed class DatabaseSizeSnapshotDto
{
    /// <summary>Gets when the snapshot was taken.</summary>
    public DateTime CapturedUtc { get; init; }

    /// <summary>Gets the PlantSafety.* database.</summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Gets the data/log file's logical name.</summary>
    public string FileLogicalName { get; init; } = string.Empty;

    /// <summary>Gets "ROWS" (data file) or "LOG" (transaction log file).</summary>
    public string FileType { get; init; } = string.Empty;

    /// <summary>Gets the allocated file size in megabytes.</summary>
    public decimal SizeMB { get; init; }

    /// <summary>Gets the actual used space in megabytes, or null if unavailable.</summary>
    public decimal? UsedSpaceMB { get; init; }
}

/// <summary>
/// Result of a SAFE, non-destructive restore-verify (msdb.dbo.usp_PlantSafety_RestoreTest).
/// This is never a production restore -- see the DR runbook link on the page for that.
/// </summary>
public sealed class RestoreVerifyResultDto
{
    /// <summary>Gets the primary key of the logged run.</summary>
    public long MaintenanceLogId { get; init; }

    /// <summary>Gets the database whose latest FULL backup was restore-tested.</summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Gets when the restore-test started.</summary>
    public DateTime StartUtc { get; init; }

    /// <summary>Gets when the restore-test finished, or null if still in flight.</summary>
    public DateTime? EndUtc { get; init; }

    /// <summary>Gets "OK" | "FAILED".</summary>
    public string Result { get; init; } = string.Empty;

    /// <summary>Gets human-readable detail (restored file, CHECKDB outcome, or the error message).</summary>
    public string? Details { get; init; }
}

/// <summary>Derived backup-health status for a single database.</summary>
public sealed class BackupHealthDto
{
    /// <summary>Gets the PlantSafety.* database.</summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Gets the timestamp of the most recent successful FULL backup, or null if none exists.</summary>
    public DateTime? LastFullBackupUtc { get; init; }

    /// <summary>Gets "Green" | "Yellow" | "Red".</summary>
    public string HealthStatus { get; init; } = string.Empty;

    /// <summary>Gets a human-readable explanation of the computed status.</summary>
    public string Reason { get; init; } = string.Empty;
}
