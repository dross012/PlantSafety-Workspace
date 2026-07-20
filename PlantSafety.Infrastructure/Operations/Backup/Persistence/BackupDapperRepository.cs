// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Backup
//  Layer:          Infrastructure
//  File:           BackupDapperRepository.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IBackupRepository. Surfaces the real
//                  SQL Server backup/DR automation built in Phase 12
//                  (msdb.dbo.usp_PlantSafety_Backup*, usp_PlantSafety_RestoreTest,
//                  PlantSafety_MaintenanceLog) and Phase 18
//                  (Invoke-PlantSafetyBackup.ps1, PlantSafety_DatabaseSizeLog).
//
//                  All data here lives in msdb -- server-level SQL Server platform
//                  state, not a PlantSafety tenant database -- so this repository opens
//                  its own SqlConnection from BackupOptions.MsdbConnectionString rather
//                  than going through ISqlConnectionFactory/DapperQueryExecutor/
//                  DapperCommandExecutor (which target the tenant-scoped primary/read
//                  connection strings and assume RLS SESSION_CONTEXT semantics that do
//                  not apply here). SqlException handling still routes through the
//                  shared ErrorClassifier so failures surface via the platform's existing
//                  exception taxonomy (ConflictException/ConcurrencyConflictException/
//                  NotFoundException/PersistenceException) rather than a parallel scheme.
//
//                  SAFETY NOTE: This repository deliberately does NOT expose a method
//                  that executes a live production RESTORE DATABASE. RunRestoreVerifyAsync
//                  only ever calls msdb.dbo.usp_PlantSafety_RestoreTest, which restores
//                  into an isolated *_DRTest database and always drops it afterwards.
//                  Actual production restore is a manual, human-run procedure -- see
//                  BackupOptions.DrRunbookPath / DR-manual-restore-runbook.md.
//
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Repositories;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Operations.Backup.Options;

namespace PlantSafety.Infrastructure.Operations.Backup.Persistence;

/// <summary>Dapper-backed repository over the real SQL Server backup/DR automation (Phase 12 + Phase 18).</summary>
public sealed class BackupDapperRepository : IBackupRepository
{
    private readonly BackupOptions _options;
    private readonly ILogger<BackupDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BackupDapperRepository(IOptions<BackupOptions> options, ILogger<BackupDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private SqlConnection CreateMsdbConnection()
    {
        var connection = new SqlConnection(_options.MsdbConnectionString);
        connection.Open();
        return connection;
    }

    /// <inheritdoc/>
    public Task<BackupTriggerResultDto> TriggerBackupAsync(BackupType backupType, CancellationToken cancellationToken)
    {
        var startedUtc = DateTime.UtcNow;

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments =
                $"-NoProfile -ExecutionPolicy Bypass -File \"{_options.BackupScriptPath}\" " +
                $"-BackupType {backupType} -ServerInstance \"{_options.ServerInstance}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Process process;
        try
        {
            process = Process.Start(psi) ?? throw new InvalidOperationException(
                "Process.Start returned null for Invoke-PlantSafetyBackup.ps1.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch Invoke-PlantSafetyBackup.ps1 for {BackupType} backup.", backupType);
            throw new PersistenceException("Invoke-PlantSafetyBackup.ps1", string.Empty, ex);
        }

        var processId = process.Id;

        // Non-blocking by design: the caller gets an immediate "Started" acknowledgement
        // rather than waiting on a potentially multi-minute FULL backup. The exit code is
        // still observed and logged on a background continuation so failures are not
        // silently lost -- callers/operators confirm real success via GetBackupHistoryAsync
        // or GetBackupHealthAsync once msdb.dbo.backupset reflects the completed run.
        _ = ObserveBackupCompletionAsync(process, backupType);

        return Task.FromResult(new BackupTriggerResultDto(
            backupType,
            processId,
            startedUtc,
            "Started",
            _options.TaskLogDirectory));
    }

    private async Task ObserveBackupCompletionAsync(Process process, BackupType backupType)
    {
        var processId = process.Id;
        try
        {
            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation(
                    "PlantSafety {BackupType} backup (PID {ProcessId}) completed successfully.",
                    backupType, processId);
            }
            else
            {
                _logger.LogError(
                    "PlantSafety {BackupType} backup (PID {ProcessId}) FAILED with exit code {ExitCode}. See {LogDirectory}.",
                    backupType, processId, process.ExitCode, _options.TaskLogDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed while observing completion of PlantSafety {BackupType} backup (PID {ProcessId}).", backupType, processId);
        }
        finally
        {
            process.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BackupHistoryItemDto>> GetBackupHistoryAsync(string? databaseName, int top, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP (@Top)
    bs.database_name       AS DatabaseName,
    bs.type                AS BackupTypeCode,
    bs.backup_finish_date  AS BackupFinishDateUtc,
    CAST(bmf.backup_size / 1048576.0 AS DECIMAL(18,2)) AS SizeMB,
    bmf.physical_device_name AS PhysicalDeviceName
FROM msdb.dbo.backupset bs
JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
WHERE bs.database_name LIKE N'PlantSafety.%'
  AND (@DatabaseName IS NULL OR bs.database_name = @DatabaseName)
ORDER BY bs.backup_finish_date DESC;";

        try
        {
            using var connection = CreateMsdbConnection();

            var rows = await connection.QueryAsync<BackupHistoryRow>(
                new CommandDefinition(
                    sql,
                    new { Top = top, DatabaseName = databaseName },
                    cancellationToken: cancellationToken));

            return rows.Select(r => new BackupHistoryItemDto(
                r.DatabaseName,
                DescribeBackupType(r.BackupTypeCode),
                r.BackupFinishDateUtc,
                r.SizeMB,
                r.PhysicalDeviceName)).ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "msdb.dbo.backupset/backupmediafamily", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DatabaseSizeSnapshotDto>> GetDatabaseSizeHistoryAsync(string? databaseName, int top, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP (@Top)
    CapturedUtc,
    DatabaseName,
    FileLogicalName,
    FileType,
    SizeMB,
    UsedSpaceMB
FROM msdb.dbo.PlantSafety_DatabaseSizeLog
WHERE (@DatabaseName IS NULL OR DatabaseName = @DatabaseName)
ORDER BY CapturedUtc DESC;";

        try
        {
            using var connection = CreateMsdbConnection();

            var rows = await connection.QueryAsync<DatabaseSizeSnapshotDto>(
                new CommandDefinition(
                    sql,
                    new { Top = top, DatabaseName = databaseName },
                    cancellationToken: cancellationToken));

            return rows.ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "msdb.dbo.PlantSafety_DatabaseSizeLog", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<RestoreVerifyResultDto> RunRestoreVerifyAsync(string databaseName, CancellationToken cancellationToken)
    {
        const string execSql = "EXEC msdb.dbo.usp_PlantSafety_RestoreTest @DatabaseName = @DatabaseName;";
        const string readBackSql = @"
SELECT TOP (1)
    MaintenanceLogId,
    DatabaseName,
    StartUtc,
    EndUtc,
    Result,
    Details
FROM msdb.dbo.PlantSafety_MaintenanceLog
WHERE TaskType = 'RESTORE_TEST' AND DatabaseName = @DatabaseName
ORDER BY StartUtc DESC;";

        try
        {
            using var connection = CreateMsdbConnection();

            // Restores a copy of the latest FULL backup into an isolated <DatabaseName>_DRTest
            // database, runs DBCC CHECKDB, then always drops the test database -- never
            // touches the live database. Long-running (backup restore + CHECKDB), hence the
            // extended command timeout.
            await connection.ExecuteAsync(
                new CommandDefinition(
                    execSql,
                    new { DatabaseName = databaseName },
                    commandTimeout: _options.RestoreVerifyCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var row = await connection.QuerySingleOrDefaultAsync<RestoreVerifyResultDto>(
                new CommandDefinition(
                    readBackSql,
                    new { DatabaseName = databaseName },
                    cancellationToken: cancellationToken));

            if (row is null)
            {
                _logger.LogWarning(
                    "usp_PlantSafety_RestoreTest ran for {DatabaseName} but no PlantSafety_MaintenanceLog row was found afterwards.",
                    databaseName);

                return new RestoreVerifyResultDto(0, databaseName, DateTime.UtcNow, DateTime.UtcNow, "UNKNOWN", "No maintenance log row found after execution.");
            }

            return row;
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "msdb.dbo.usp_PlantSafety_RestoreTest", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BackupHealthDto>> GetBackupHealthAsync(CancellationToken cancellationToken)
    {
        // Live-verified inventory approach mirrors Phase 12/120 and Phase 18/180: PlantSafety.*
        // databases with state_desc = 'ONLINE' and database_id > 4 (i.e. not a system database).
        const string sql = @"
SELECT
    d.name AS DatabaseName,
    b.LastFullBackupUtc
FROM sys.databases d
LEFT JOIN (
    SELECT database_name, MAX(backup_finish_date) AS LastFullBackupUtc
    FROM msdb.dbo.backupset
    WHERE type = 'D'
    GROUP BY database_name
) b ON b.database_name = d.name
WHERE d.name LIKE N'PlantSafety.%'
  AND d.state_desc = 'ONLINE'
  AND d.database_id > 4
ORDER BY d.name;";

        try
        {
            using var connection = CreateMsdbConnection();

            var rows = await connection.QueryAsync<BackupHealthRow>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            var nowUtc = DateTime.UtcNow;

            return rows.Select(r => BuildHealth(r, nowUtc)).ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "sys.databases/msdb.dbo.backupset (health)", _logger);
        }
    }

    private BackupHealthDto BuildHealth(BackupHealthRow row, DateTime nowUtc)
    {
        if (row.LastFullBackupUtc is null)
        {
            return new BackupHealthDto(
                row.DatabaseName,
                null,
                "Red",
                "No successful FULL backup has ever completed for this database.");
        }

        var ageHours = (nowUtc - row.LastFullBackupUtc.Value).TotalHours;

        if (ageHours <= _options.HealthWarnThresholdHours)
        {
            return new BackupHealthDto(
                row.DatabaseName,
                row.LastFullBackupUtc,
                "Green",
                $"Last FULL backup {ageHours:F1}h ago (within {_options.HealthWarnThresholdHours}h threshold).");
        }

        if (ageHours <= _options.HealthCriticalThresholdHours)
        {
            return new BackupHealthDto(
                row.DatabaseName,
                row.LastFullBackupUtc,
                "Yellow",
                $"Last FULL backup {ageHours:F1}h ago (older than {_options.HealthWarnThresholdHours}h, within {_options.HealthCriticalThresholdHours}h critical threshold).");
        }

        return new BackupHealthDto(
            row.DatabaseName,
            row.LastFullBackupUtc,
            "Red",
            $"Last FULL backup {ageHours:F1}h ago (older than the {_options.HealthCriticalThresholdHours}h critical threshold).");
    }

    private static string DescribeBackupType(string typeCode) => typeCode switch
    {
        "D" => "Full",
        "I" => "Differential",
        "L" => "Transaction Log",
        _ => typeCode
    };

    /// <summary>Raw projection of msdb.dbo.backupset/backupmediafamily before friendly-name mapping.</summary>
    private sealed class BackupHistoryRow
    {
        public string DatabaseName { get; init; } = string.Empty;
        public string BackupTypeCode { get; init; } = string.Empty;
        public DateTime BackupFinishDateUtc { get; init; }
        public decimal SizeMB { get; init; }
        public string PhysicalDeviceName { get; init; } = string.Empty;
    }

    /// <summary>Raw projection of the sys.databases/backupset health query.</summary>
    private sealed class BackupHealthRow
    {
        public string DatabaseName { get; init; } = string.Empty;
        public DateTime? LastFullBackupUtc { get; init; }
    }
}
