// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-19
// FILE DESCRIPTION: Typed API client for the Operations.Backup module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.Backup.Controllers.V1
//   .OperationsBackupV1Controller. Deliberately exposes NO method that triggers a
//   live production restore -- RunRestoreVerifyAsync only ever calls the safe,
//   isolated restore-verify endpoint. See DR-manual-restore-runbook.md for the
//   human-run production restore procedure.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.OperationsBackup.Models;

namespace PlantSafety.UI.Modules.OperationsBackup.ApiClients;

/// <summary>Typed API client for the Operations.Backup module.</summary>
public sealed class BackupApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="BackupApiClient"/>.</summary>
    public BackupApiClient(HttpClient http, ILogger<BackupApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Triggers an on-demand backup (Full/Diff/Log). Non-blocking on the server side.</summary>
    public Task<ApiResponseEnvelope<BackupTriggerResultDto>> TriggerBackupAsync(
        BackupType backupType,
        CancellationToken cancellationToken = default)
        => PostAsync<object, BackupTriggerResultDto>(
            "v1/operations/backup/trigger",
            new { BackupType = backupType },
            cancellationToken);

    /// <summary>Retrieves recent backup history, newest first.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<BackupHistoryItemDto>>> GetHistoryAsync(
        string? databaseName = null,
        int top = 50,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<BackupHistoryItemDto>>(
            $"v1/operations/backup/history?top={top}" + (string.IsNullOrWhiteSpace(databaseName) ? string.Empty : $"&databaseName={Uri.EscapeDataString(databaseName)}"),
            cancellationToken);

    /// <summary>Retrieves recent database size snapshots.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<DatabaseSizeSnapshotDto>>> GetDatabaseSizeAsync(
        string? databaseName = null,
        int top = 100,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<DatabaseSizeSnapshotDto>>(
            $"v1/operations/backup/database-size?top={top}" + (string.IsNullOrWhiteSpace(databaseName) ? string.Empty : $"&databaseName={Uri.EscapeDataString(databaseName)}"),
            cancellationToken);

    /// <summary>
    /// Runs a SAFE, isolated restore-verify against the given database's latest FULL
    /// backup. Never touches the live database. NOT a production restore.
    /// </summary>
    public Task<ApiResponseEnvelope<RestoreVerifyResultDto>> RunRestoreVerifyAsync(
        string databaseName,
        CancellationToken cancellationToken = default)
        => PostAsync<object, RestoreVerifyResultDto>(
            "v1/operations/backup/restore-verify",
            new { DatabaseName = databaseName },
            cancellationToken);

    /// <summary>Retrieves derived backup-health status for every live PlantSafety.* database.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<BackupHealthDto>>> GetHealthAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<BackupHealthDto>>("v1/operations/backup/health", cancellationToken);
}
