// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Monitoring
//  Layer:          Infrastructure
//  File:           MonitoringDapperRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IMonitoringRepository.
//                    - DB Performance: queries sys.query_store_query /
//                      sys.query_store_query_text / sys.query_store_plan /
//                      sys.query_store_runtime_stats / sys.query_store_runtime_stats_interval
//                      per Microsoft's documented Query Store schema, against the target
//                      database identified by MonitoringOptions.DatabaseConnectionStrings.
//                    - DB Errors: queries [PlantSafety.ErrorDB].dbo.ErrorLog directly
//                      (columns confirmed live from Phase17,
//                      170-fix-errorlog-masking-bug.sql's INSERT statements).
//                    - DB Deadlocks: reads the built-in "system_health" Extended Events
//                      session's ring_buffer target via sys.dm_xe_session_targets /
//                      sys.dm_xe_sessions and shreds xml_deadlock_report events with XQuery,
//                      then parses each &lt;deadlock&gt; fragment client-side with
//                      System.Xml.Linq for the participant count / resource summary.
//
//                  All three surfaces are server-level SQL Server platform state, not
//                  tenant-scoped RLS data, so this repository opens its own SqlConnection
//                  per call rather than going through ISqlConnectionFactory -- same
//                  reasoning as Operations.Backup/SchemaGuard. SqlException handling
//                  routes through the shared ErrorClassifier. Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Repositories;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Operations.Monitoring.Options;

namespace PlantSafety.Infrastructure.Operations.Monitoring.Persistence;

/// <summary>Dapper-backed repository over DB Performance / DB Errors / DB Deadlocks monitoring surfaces.</summary>
public sealed class MonitoringDapperRepository : IMonitoringRepository
{
    private readonly MonitoringOptions _options;
    private readonly ILogger<MonitoringDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public MonitoringDapperRepository(IOptions<MonitoringOptions> options, ILogger<MonitoringDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<QueryPerformanceItemDto>> GetTopQueriesByCpuAsync(
        string databaseKey, int top, int lookbackHours, CancellationToken cancellationToken)
    {
        if (!_options.DatabaseConnectionStrings.TryGetValue(databaseKey, out var connectionString))
        {
            _logger.LogWarning("Query Store performance check: unknown database key {DatabaseKey}.", databaseKey);
            return [];
        }

        const string sql = @"
SELECT TOP (@Top)
    q.query_id                                   AS QueryId,
    p.plan_id                                     AS PlanId,
    qt.query_sql_text                              AS QueryText,
    SUM(rs.count_executions)                       AS ExecutionCount,
    AVG(rs.avg_duration) / 1000.0                  AS AvgDurationMs,
    AVG(rs.avg_cpu_time) / 1000.0                  AS AvgCpuTimeMs,
    AVG(rs.avg_logical_io_reads)                   AS AvgLogicalReads,
    MAX(rsi.end_time)                              AS LastExecutionUtc
FROM sys.query_store_query q
JOIN sys.query_store_query_text qt ON qt.query_text_id = q.query_text_id
JOIN sys.query_store_plan p ON p.query_id = q.query_id
JOIN sys.query_store_runtime_stats rs ON rs.plan_id = p.plan_id
JOIN sys.query_store_runtime_stats_interval rsi ON rsi.runtime_stats_interval_id = rs.runtime_stats_interval_id
WHERE rsi.start_time >= DATEADD(HOUR, -@LookbackHours, SYSUTCDATETIME())
GROUP BY q.query_id, p.plan_id, qt.query_sql_text
ORDER BY AVG(rs.avg_cpu_time) DESC;";

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var rows = await connection.QueryAsync<QueryPerformanceRow>(
                new CommandDefinition(
                    sql,
                    new { Top = top, LookbackHours = lookbackHours },
                    cancellationToken: cancellationToken));

            return rows.Select(r => new QueryPerformanceItemDto(
                databaseKey,
                r.QueryId,
                r.PlanId,
                Truncate(r.QueryText, 500),
                r.ExecutionCount,
                r.AvgDurationMs,
                r.AvgCpuTimeMs,
                r.AvgLogicalReads,
                r.LastExecutionUtc)).ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, $"sys.query_store_* ({databaseKey})", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ErrorLogItemDto>> GetRecentErrorsAsync(
        int? minSeverity,
        DateTime? sinceUtc,
        int top,
        int skip,
        CancellationToken cancellationToken,
        int? maxSeverity = null,
        string? sourceDatabase = null,
        string? module = null,
        string? errorProcedure = null)
    {
        // Severity band: MinSeverity/MaxSeverity are both inclusive, mirroring SQL Server's own
        // documented severity bands (0-10 informational, 11-16 user-correctable/typical app
        // errors, 17-25 severe/system) so callers can filter e.g. MinSeverity=17 to see only
        // severe errors, or MinSeverity=11,MaxSeverity=16 to see only typical app errors.
        const string sql = @"
SELECT
    ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
    ErrorDate AS ErrorDateUtc, ErrorContext, ErrorParameters, SourceDatabase, Module
FROM [PlantSafety.ErrorDB].dbo.ErrorLog
WHERE (@MinSeverity IS NULL OR ErrorSeverity >= @MinSeverity)
  AND (@MaxSeverity IS NULL OR ErrorSeverity <= @MaxSeverity)
  AND (@SinceUtc IS NULL OR ErrorDate >= @SinceUtc)
  AND (@SourceDatabase IS NULL OR SourceDatabase = @SourceDatabase)
  AND (@Module IS NULL OR Module = @Module)
  AND (@ErrorProcedure IS NULL OR ErrorProcedure = @ErrorProcedure)
ORDER BY ErrorDate DESC
OFFSET @Skip ROWS FETCH NEXT @Top ROWS ONLY;";

        try
        {
            await using var connection = new SqlConnection(_options.ErrorDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var rows = await connection.QueryAsync<ErrorLogItemDto>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        MinSeverity = minSeverity,
                        MaxSeverity = maxSeverity,
                        SinceUtc = sinceUtc,
                        SourceDatabase = sourceDatabase,
                        Module = module,
                        ErrorProcedure = errorProcedure,
                        Top = top,
                        Skip = skip
                    },
                    cancellationToken: cancellationToken));

            return rows.ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "[PlantSafety.ErrorDB].dbo.ErrorLog", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DeadlockEventDto>> GetRecentDeadlocksAsync(int top, CancellationToken cancellationToken)
    {
        // Reads the ring_buffer target of the built-in "system_health" session -- there is
        // no custom deadlock-capturing session defined by Phase13
        // (131-extended-events-sessions.sql only adds PlantSafety_ErrorCapture and
        // PlantSafety_LongRunningQueries, neither of which subscribes to
        // sqlserver.xml_deadlock_report).
        const string sql = @"
;WITH XESessionData AS (
    SELECT CAST(st.target_data AS XML) AS TargetData
    FROM sys.dm_xe_session_targets st
    JOIN sys.dm_xe_sessions s ON s.address = st.event_session_address
    WHERE s.name = @SessionName
      AND st.target_name = N'ring_buffer'
),
Events AS (
    SELECT
        x.value('(@timestamp)[1]', 'datetime2')                                             AS EventTimeUtc,
        x.query('data[@name=""xml_report""]/value/deadlock')                                  AS DeadlockXml
    FROM XESessionData
    CROSS APPLY TargetData.nodes('RingBufferTarget/event[@name=""xml_deadlock_report""]') AS T(x)
)
SELECT TOP (@Top)
    EventTimeUtc,
    CAST(DeadlockXml AS NVARCHAR(MAX)) AS DeadlockXml
FROM Events
ORDER BY EventTimeUtc DESC;";

        try
        {
            await using var connection = new SqlConnection(_options.ServerConnectionString);
            await connection.OpenAsync(cancellationToken);

            var rows = await connection.QueryAsync<DeadlockRow>(
                new CommandDefinition(
                    sql,
                    new { SessionName = _options.DeadlockSessionName, Top = top },
                    cancellationToken: cancellationToken));

            return rows.Select(ParseDeadlockRow).ToList();
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, $"sys.dm_xe_session_targets ({_options.DeadlockSessionName})", _logger);
        }
    }

    private DeadlockEventDto ParseDeadlockRow(DeadlockRow row)
    {
        try
        {
            var doc = XElement.Parse(row.DeadlockXml);

            var victimId = doc.Element("victim-list")?.Element("victimProcess")?.Attribute("id")?.Value;

            var participantCount = doc.Element("process-list")?.Elements("process").Count() ?? 0;

            var resourceList = doc.Element("resource-list");
            var resourceSummary = resourceList is null
                ? "Unknown"
                : string.Join("; ", resourceList.Elements().Select(DescribeResource));

            return new DeadlockEventDto(row.EventTimeUtc, victimId, participantCount, resourceSummary, row.DeadlockXml);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse xml_deadlock_report XML for event at {EventTimeUtc}.", row.EventTimeUtc);
            return new DeadlockEventDto(row.EventTimeUtc, null, 0, "Unable to parse deadlock XML.", row.DeadlockXml);
        }
    }

    private static string DescribeResource(XElement resource)
    {
        var objectName = resource.Attribute("objectname")?.Value;
        var resourceType = resource.Name.LocalName;
        return objectName is null ? resourceType : $"{resourceType} ({objectName})";
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "…";

    /// <summary>Raw projection of the Query Store query before database-key annotation.</summary>
    private sealed class QueryPerformanceRow
    {
        public long QueryId { get; init; }
        public long PlanId { get; init; }
        public string QueryText { get; init; } = string.Empty;
        public long ExecutionCount { get; init; }
        public double AvgDurationMs { get; init; }
        public double AvgCpuTimeMs { get; init; }
        public double AvgLogicalReads { get; init; }
        public DateTime? LastExecutionUtc { get; init; }
    }

    /// <summary>Raw projection of a shredded xml_deadlock_report event before XML parsing.</summary>
    private sealed class DeadlockRow
    {
        public DateTime EventTimeUtc { get; init; }
        public string DeadlockXml { get; init; } = string.Empty;
    }
}
