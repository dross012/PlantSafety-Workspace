// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Monitoring
//  Layer:          Application
//  File:           IMonitoringRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the platform's DB Performance (Query
//                  Store), DB Errors ([PlantSafety.ErrorDB].dbo.ErrorLog), and DB
//                  Deadlocks ("system_health" XE session) monitoring surfaces.
//
//                  Deliberately does NOT re-implement DB Health (see
//                  GetDatabaseHealthReportQuery, which calls the existing
//                  Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService
//                  directly) or DB Size (see GetMonitoringDatabaseSizeQuery, which
//                  delegates to the existing
//                  PlantSafety.Application.Operations.Backup.Repositories.IBackupRepository
//                  .GetDatabaseSizeHistoryAsync rather than re-querying
//                  msdb.dbo.PlantSafety_DatabaseSizeLog a second time).
//
//                  Implemented in the Infrastructure layer. All data here is server-level
//                  SQL Server platform state (per-database Query Store DMVs, a shared
//                  cross-database ErrorLog table, and server-scoped XE session state) —
//                  NOT tenant-scoped, so this repository opens its own SqlConnection(s)
//                  from MonitoringOptions rather than going through ISqlConnectionFactory,
//                  mirroring Operations.Backup / Operations.SchemaGuard. Additive-only
//                  evolution.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.Monitoring.Repositories;

/// <summary>Repository over DB Performance / DB Errors / DB Deadlocks monitoring surfaces.</summary>
public interface IMonitoringRepository
{
    /// <summary>
    /// Returns the top <paramref name="top"/> queries by average CPU time for
    /// <paramref name="databaseKey"/> over the trailing <paramref name="lookbackHours"/>,
    /// sourced from that database's Query Store (sys.query_store_query /
    /// sys.query_store_query_text / sys.query_store_plan / sys.query_store_runtime_stats /
    /// sys.query_store_runtime_stats_interval). Query Store must be ON for the target
    /// database (Phase 11, 111-query-store-configuration.sql).
    /// </summary>
    /// <param name="databaseKey">Logical database key (see MonitoringOptions.DatabaseConnectionStrings).</param>
    /// <param name="top">Maximum number of rows to return.</param>
    /// <param name="lookbackHours">How far back to look, in hours, based on runtime-stats interval start time.</param>
    Task<IReadOnlyList<QueryPerformanceItemDto>> GetTopQueriesByCpuAsync(
        string databaseKey, int top, int lookbackHours, CancellationToken cancellationToken);

    /// <summary>
    /// Returns recent rows from [PlantSafety.ErrorDB].dbo.ErrorLog, newest first, with
    /// optional severity-band, date, source-database, module, and procedure filters,
    /// paginated via top/skip.
    /// </summary>
    /// <param name="minSeverity">Optional minimum ErrorSeverity (inclusive). Null returns all severities.</param>
    /// <param name="sinceUtc">Optional lower bound on ErrorDate (inclusive). Null returns all history.</param>
    /// <param name="top">Maximum number of rows to return (page size).</param>
    /// <param name="skip">Number of rows to skip (for pagination).</param>
    /// <param name="maxSeverity">Optional maximum ErrorSeverity (inclusive), paired with <paramref name="minSeverity"/> for a severity band.</param>
    /// <param name="sourceDatabase">Optional exact match on the SourceDatabase column.</param>
    /// <param name="module">Optional exact match on the Module column.</param>
    /// <param name="errorProcedure">Optional exact match on the ErrorProcedure column.</param>
    Task<IReadOnlyList<ErrorLogItemDto>> GetRecentErrorsAsync(
        int? minSeverity,
        System.DateTime? sinceUtc,
        int top,
        int skip,
        CancellationToken cancellationToken,
        int? maxSeverity = null,
        string? sourceDatabase = null,
        string? module = null,
        string? errorProcedure = null);

    /// <summary>
    /// Returns the most recent <paramref name="top"/> deadlock events parsed from the
    /// built-in "system_health" Extended Events session's ring_buffer target, newest first.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <param name="top">Maximum number of rows to return.</param>
    Task<IReadOnlyList<DeadlockEventDto>> GetRecentDeadlocksAsync(int top, CancellationToken cancellationToken);
}
