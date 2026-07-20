// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Archival
//  Layer:          Infrastructure
//  File:           ArchivalDapperRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IArchivalRepository. Surfaces the real
//                  Phase7 Governance retention/purge automation (74-retention-archival.sql:
//                  dbo.sp_AuditLog_Purge, dbo.sp_ErrorLog_Purge,
//                  Integration.sp_IntegrationLogEntry_Purge, Integration.sp_ReplayRecord_Purge,
//                  Diagnostics.sp_DiagnosticEvent_Purge) and the
//                  [PlantSafety.ArchiveDB].Archival.ArchivalJob / Archival.TierPolicy tables.
//
//                  TRIGGER STRATEGY -- direct EXEC via Dapper, NOT Process.Start (contrast
//                  with BackupDapperRepository.TriggerBackupAsync):
//
//                    * Each purge procedure is a single, fast, bounded DELETE statement
//                      (rows older than a TierPolicy-governed cutoff) followed by one
//                      ArchivalJob INSERT logging Completed/Blocked/Failed. It is not a
//                      multi-minute, resource-intensive operation like a FULL database
//                      BACKUP, so there is no need to fire-and-forget it in a background
//                      process and poll for completion afterwards.
//                    * A direct, synchronous EXEC lets the caller receive the procedure's
//                      REAL, already-logged outcome (Completed/Blocked/Failed) in the same
//                      request/response cycle, instead of an ambiguous "Started"
//                      acknowledgement that must be reconciled later.
//                    * These procedures already run unattended today (Phase 18 Task
//                      Scheduler, Invoke-PlantSafetyPurge.ps1 -> sqlcmd -Q "EXEC <proc>").
//                      Calling EXEC directly from C# reuses the exact same, already-approved
//                      SQL entry point (no duplicated purge logic), just via ADO.NET instead
//                      of a shelled-out sqlcmd process -- a strictly lower-risk substitution,
//                      not a new capability.
//                    * Unlike BackupDapperRepository.RunRestoreVerifyAsync's cousin (a real
//                      production RESTORE would be irreversible and slow), these procedures
//                      can only ever delete rows OLDER than a configured retention cutoff and
//                      are idempotent -- running one twice in a row purges nothing extra the
//                      second time. sp_ErrorLog_Purge is additionally hard-blocked at the
//                      database level by TR_ErrorLog_NoDelete regardless of caller.
//
//                  All data here lives in per-database server-level tables
//                  (PlantSafety.AuditDB/ErrorDB/IntegrationDB/DiagnosticsDB/ArchiveDB), so
//                  this repository opens its own SqlConnection per database key from
//                  ArchivalOptions.DatabaseConnectionStrings rather than going through
//                  ISqlConnectionFactory/DapperQueryExecutor/DapperCommandExecutor (tenant-
//                  scoped, RLS SESSION_CONTEXT semantics do not apply here). SqlException
//                  handling routes through the shared ErrorClassifier.
//
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Repositories;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Operations.Archival.Options;

namespace PlantSafety.Infrastructure.Operations.Archival.Persistence;

/// <summary>Dapper-backed repository over the real Phase 7 Governance retention/purge automation.</summary>
public sealed class ArchivalDapperRepository : IArchivalRepository
{
    /// <summary>Fixed, known inventory of the 5 live purge targets, matching 74-retention-archival.sql exactly.</summary>
    private static readonly IReadOnlyList<TargetDefinition> Targets =
    [
        new(PurgeTarget.AuditLog, "AuditDB.dbo.AuditLog", "AuditDB", "PlantSafety.AuditDB", "dbo.sp_AuditLog_Purge",
            "Purges dbo.AuditLog rows older than the configured retention window."),
        new(PurgeTarget.ErrorLog, "ErrorDB.dbo.ErrorLog", "ErrorDB", "PlantSafety.ErrorDB", "dbo.sp_ErrorLog_Purge",
            "Attempts to purge dbo.ErrorLog rows, but is always blocked by the TR_ErrorLog_NoDelete immutability trigger. This is a designed, logged 'Blocked' outcome, not a failure."),
        new(PurgeTarget.IntegrationLogEntry, "IntegrationDB.Integration.IntegrationLogEntry", "IntegrationDB", "PlantSafety.IntegrationDB", "Integration.sp_IntegrationLogEntry_Purge",
            "Purges Integration.IntegrationLogEntry rows older than the configured retention window."),
        new(PurgeTarget.ReplayRecord, "IntegrationDB.Integration.ReplayRecord", "IntegrationDB", "PlantSafety.IntegrationDB", "Integration.sp_ReplayRecord_Purge",
            "Purges Integration.ReplayRecord rows older than the configured retention window."),
        new(PurgeTarget.DiagnosticEvent, "DiagnosticsDB.Diagnostics.DiagnosticEvent", "DiagnosticsDB", "PlantSafety.DiagnosticsDB", "Diagnostics.sp_DiagnosticEvent_Purge",
            "Purges Diagnostics.DiagnosticEvent rows older than the configured retention window."),
    ];

    private readonly ArchivalOptions _options;
    private readonly ILogger<ArchivalDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchivalDapperRepository(IOptions<ArchivalOptions> options, ILogger<ArchivalDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PurgeTargetDto> GetPurgeTargets() =>
        Targets.Select(t => new PurgeTargetDto(t.Target, t.Zone, t.DisplayDatabaseName, t.ProcedureName, t.Description)).ToList();

    private SqlConnection CreateConnection(string databaseKey)
    {
        if (!_options.DatabaseConnectionStrings.TryGetValue(databaseKey, out var connectionString))
        {
            throw new InvalidOperationException($"No connection string configured for Operations:Archival database key '{databaseKey}'.");
        }

        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurgeTriggerResultDto>> TriggerPurgeAsync(PurgeTarget? target, CancellationToken cancellationToken)
    {
        var targetsToRun = target is null ? Targets : Targets.Where(t => t.Target == target.Value).ToList();
        var results = new List<PurgeTriggerResultDto>(targetsToRun.Count);

        foreach (var definition in targetsToRun)
        {
            results.Add(await RunSingleTargetAsync(definition, cancellationToken));
        }

        return results;
    }

    private async Task<PurgeTriggerResultDto> RunSingleTargetAsync(TargetDefinition definition, CancellationToken cancellationToken)
    {
        try
        {
            using (var connection = CreateConnection(definition.DatabaseKey))
            {
                // Direct EXEC of the same, already-scheduled purge procedure -- see the
                // class-level SAFETY NOTE for why this is a safe, low-risk on-demand trigger.
                await connection.ExecuteAsync(new CommandDefinition(
                    $"EXEC {definition.ProcedureName};",
                    commandTimeout: _options.PurgeCommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
            }

            // Read back the ArchivalJob row the procedure itself just logged, from
            // PlantSafety.ArchiveDB (a separate database from the one the proc ran in).
            using var archiveConnection = CreateConnection("ArchiveDB");

            const string readBackSql = @"
SELECT TOP (1)
    ArchivalJobId, Status, StartedAtUtc, CompletedAtUtc, FailureReason
FROM Archival.ArchivalJob
WHERE PartitionKey = @Zone AND ISNULL(IsDeleted, 0) = 0
ORDER BY StartedAtUtc DESC;";

            var row = await archiveConnection.QuerySingleOrDefaultAsync<ArchivalJobReadBackRow>(
                new CommandDefinition(readBackSql, new { definition.Zone }, cancellationToken: cancellationToken));

            if (row is null)
            {
                _logger.LogWarning(
                    "{ProcedureName} ran for zone {Zone} but no Archival.ArchivalJob row was found afterwards.",
                    definition.ProcedureName, definition.Zone);

                return new PurgeTriggerResultDto(definition.Target, definition.Zone, "Unknown", null, null, null,
                    "No Archival.ArchivalJob row was found after execution.");
            }

            if (string.Equals(row.Status, "Blocked", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "{ProcedureName} for zone {Zone} was blocked as designed (see TR_ErrorLog_NoDelete). Detail: {Detail}",
                    definition.ProcedureName, definition.Zone, row.FailureReason);
            }
            else if (string.Equals(row.Status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    "{ProcedureName} for zone {Zone} FAILED. Detail: {Detail}",
                    definition.ProcedureName, definition.Zone, row.FailureReason);
            }
            else
            {
                _logger.LogInformation(
                    "{ProcedureName} for zone {Zone} completed successfully. Detail: {Detail}",
                    definition.ProcedureName, definition.Zone, row.FailureReason);
            }

            return new PurgeTriggerResultDto(definition.Target, definition.Zone, row.Status, row.ArchivalJobId, row.StartedAtUtc, row.CompletedAtUtc, row.FailureReason);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, definition.ProcedureName, _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ArchivalJobDto>> GetPurgeStatusAsync(string? zone, int top, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP (@Top)
    ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status,
    StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy
FROM Archival.ArchivalJob
WHERE ISNULL(IsDeleted, 0) = 0
  AND (@Zone IS NULL OR PartitionKey = @Zone)
ORDER BY StartedAtUtc DESC;";

        try
        {
            using var connection = CreateConnection("ArchiveDB");

            var rows = await connection.QueryAsync<ArchivalJobDto>(
                new CommandDefinition(sql, new { Top = top, Zone = zone }, cancellationToken: cancellationToken));

            return rows.ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Archival.ArchivalJob", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TierPolicyDto>> GetRetentionSettingsAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TierPolicyId, Zone, SourceTier, TargetTier, TransitionAfterDays, CreatedUtc, CreatedBy
FROM Archival.TierPolicy
WHERE ISNULL(IsDeleted, 0) = 0
ORDER BY Zone;";

        try
        {
            using var connection = CreateConnection("ArchiveDB");

            var rows = await connection.QueryAsync<TierPolicyDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return rows.ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Archival.TierPolicy", _logger);
        }
    }

    /// <summary>Fixed metadata for one of the 5 live purge targets.</summary>
    private sealed record TargetDefinition(
        PurgeTarget Target,
        string Zone,
        string DatabaseKey,
        string DisplayDatabaseName,
        string ProcedureName,
        string Description);

    /// <summary>Raw projection of the post-EXEC Archival.ArchivalJob read-back.</summary>
    private sealed class ArchivalJobReadBackRow
    {
        public Guid ArchivalJobId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public string? FailureReason { get; init; }
    }
}
