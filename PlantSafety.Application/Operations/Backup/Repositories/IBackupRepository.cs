// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           IBackupRepository.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real SQL Server backup/DR automation
//                  built in Phase 12 (msdb.dbo.usp_PlantSafety_Backup*, usp_PlantSafety_RestoreTest)
//                  and Phase 18 (Invoke-PlantSafetyBackup.ps1, PlantSafety_DatabaseSizeLog).
//                  Implemented in the Infrastructure layer against msdb — NOT tenant-scoped,
//                  server-level SQL Server platform data. Additive-only evolution.
//
//  NOT to be confused with PlantSafety.Application.CrossDomain.Backup.* — that is an
//  unrelated generic scaffolded CRUD stub for a domain entity literally named "Backup".
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Repositories;

/// <summary>Repository over the real SQL Server backup/DR automation (Phase 12 + Phase 18).</summary>
public interface IBackupRepository
{
    /// <summary>
    /// Launches Invoke-PlantSafetyBackup.ps1 -BackupType &lt;backupType&gt; as a non-blocking
    /// child process. Returns an immediate "Started" acknowledgement; the eventual
    /// success/failure of the backup itself is observed via <see cref="GetBackupHistoryAsync"/>
    /// or <see cref="GetBackupHealthAsync"/> after it completes.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <param name="backupType">Backup type to filter by, or null for all types.</param>
    Task<BackupTriggerResultDto> TriggerBackupAsync(BackupType backupType, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the most recent backups from msdb.dbo.backupset / backupmediafamily,
    /// optionally filtered to a single database, newest first.
    /// </summary>
    /// <param name="databaseName">Optional exact database name filter. Null returns all PlantSafety.* databases.</param>
    /// <param name="top">Maximum number of rows to return.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task<IReadOnlyList<BackupHistoryItemDto>> GetBackupHistoryAsync(string? databaseName, int top, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the most recent database size snapshots from
    /// msdb.dbo.PlantSafety_DatabaseSizeLog, optionally filtered to a single database.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <param name="databaseName">Name of the database the operation targets.</param>
    /// <param name="top">Maximum number of rows to return.</param>
    Task<IReadOnlyList<DatabaseSizeSnapshotDto>> GetDatabaseSizeHistoryAsync(string? databaseName, int top, CancellationToken cancellationToken);

    /// <summary>
    /// Runs the SAFE, non-destructive restore-verify (msdb.dbo.usp_PlantSafety_RestoreTest)
    /// against <paramref name="databaseName"/>: restores the latest FULL backup into an
    /// isolated, self-cleaning &lt;databaseName&gt;_DRTest database, runs DBCC CHECKDB, then
    /// always drops the test database. Never modifies the live database.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <param name="databaseName">Name of the database the operation targets.</param>
    Task<RestoreVerifyResultDto> RunRestoreVerifyAsync(string databaseName, CancellationToken cancellationToken);

    /// <summary>
    /// Derives a backup-health status (Green/Yellow/Red) for every live PlantSafety.*
    /// database from the recency of its most recent successful FULL backup.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task<IReadOnlyList<BackupHealthDto>> GetBackupHealthAsync(CancellationToken cancellationToken);
}
