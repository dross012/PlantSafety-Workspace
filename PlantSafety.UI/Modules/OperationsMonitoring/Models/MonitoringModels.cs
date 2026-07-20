// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.Monitoring module. Mirror
//   PlantSafety.Application.Operations.Monitoring.Models.* (and
//   PlantSafety.Application.Operations.Backup.Models.DatabaseSizeSnapshotDto, reused
//   as-is for DB Size) returned by
//   PlantSafety.Api.Operations.Monitoring.Controllers.V1.OperationsMonitoringV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.OperationsMonitoring.Models;

/// <summary>Aggregated snapshot of the platform's /health/ready readiness report.</summary>
public sealed class DatabaseHealthReportDto
{
    /// <summary>Gets "Healthy" | "Degraded" | "Unhealthy".</summary>
    public string OverallStatus { get; init; } = string.Empty;

    /// <summary>Gets when the report was produced.</summary>
    public DateTime CheckedUtc { get; init; }

    /// <summary>Gets the total wall-clock time spent running all registered health checks.</summary>
    public double TotalDurationMs { get; init; }

    /// <summary>Gets per-check detail (database, redis, servicebus, schema-version, ...).</summary>
    public List<HealthCheckEntryDto> Entries { get; init; } = [];
}

/// <summary>A single named entry within <see cref="DatabaseHealthReportDto"/>.</summary>
public sealed class HealthCheckEntryDto
{
    /// <summary>Gets the registered health check name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets "Healthy" | "Degraded" | "Unhealthy".</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets human-readable detail returned by the check, if any.</summary>
    public string? Description { get; init; }

    /// <summary>Gets how long this individual check took to run.</summary>
    public double DurationMs { get; init; }

    /// <summary>Gets the tags the check was registered with.</summary>
    public List<string> Tags { get; init; } = [];
}

/// <summary>A single database size snapshot, reused from Operations.Backup's msdb.dbo.PlantSafety_DatabaseSizeLog.</summary>
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

/// <summary>A single Query Store row summarizing recent execution statistics for one query/plan.</summary>
public sealed class QueryPerformanceItemDto
{
    /// <summary>Gets the logical database key.</summary>
    public string DatabaseKey { get; init; } = string.Empty;

    /// <summary>Gets sys.query_store_query.query_id.</summary>
    public long QueryId { get; init; }

    /// <summary>Gets sys.query_store_plan.plan_id.</summary>
    public long PlanId { get; init; }

    /// <summary>Gets the query's SQL text, truncated for display.</summary>
    public string QueryText { get; init; } = string.Empty;

    /// <summary>Gets total executions across the runtime-stats intervals considered.</summary>
    public long ExecutionCount { get; init; }

    /// <summary>Gets the average duration in milliseconds.</summary>
    public double AvgDurationMs { get; init; }

    /// <summary>Gets the average CPU time in milliseconds.</summary>
    public double AvgCpuTimeMs { get; init; }

    /// <summary>Gets the average logical page reads.</summary>
    public double AvgLogicalReads { get; init; }

    /// <summary>Gets the end time of the most recent runtime-stats interval observed.</summary>
    public DateTime? LastExecutionUtc { get; init; }
}

/// <summary>A single row from [PlantSafety.ErrorDB].dbo.ErrorLog.</summary>
public sealed class ErrorLogItemDto
{
    /// <summary>Gets SQL Server ERROR_NUMBER().</summary>
    public int? ErrorNumber { get; init; }

    /// <summary>Gets SQL Server ERROR_SEVERITY().</summary>
    public int? ErrorSeverity { get; init; }

    /// <summary>Gets SQL Server ERROR_STATE().</summary>
    public int? ErrorState { get; init; }

    /// <summary>Gets the stored procedure that raised the error, if any.</summary>
    public string? ErrorProcedure { get; init; }

    /// <summary>Gets the line within the procedure that raised the error.</summary>
    public int? ErrorLine { get; init; }

    /// <summary>Gets SQL Server ERROR_MESSAGE().</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets when the error was logged.</summary>
    public DateTime ErrorDateUtc { get; init; }

    /// <summary>Gets caller-supplied parameter context.</summary>
    public string? ErrorContext { get; init; }

    /// <summary>Gets the structured detail string.</summary>
    public string? ErrorParameters { get; init; }

    /// <summary>Gets the database the failing procedure executed against.</summary>
    public string? SourceDatabase { get; init; }

    /// <summary>Gets the stored procedure/object name.</summary>
    public string? Module { get; init; }
}

/// <summary>Filter state for the DB Errors panel's search form.</summary>
public sealed class ErrorLogFilter
{
    /// <summary>Gets or sets the inclusive minimum ErrorSeverity.</summary>
    public int? MinSeverity { get; set; }

    /// <summary>Gets or sets the inclusive maximum ErrorSeverity, paired with <see cref="MinSeverity"/> for a severity band.</summary>
    public int? MaxSeverity { get; set; }

    /// <summary>Gets or sets the inclusive lower bound on ErrorDate (UTC).</summary>
    public DateTime? SinceUtc { get; set; }

    /// <summary>Gets or sets the exact SourceDatabase filter.</summary>
    public string? SourceDatabase { get; set; }

    /// <summary>Gets or sets the exact Module (stored procedure/object name) filter.</summary>
    public string? Module { get; set; }

    /// <summary>Gets or sets the exact ErrorProcedure filter.</summary>
    public string? ErrorProcedure { get; set; }
}

/// <summary>A single parsed "Key=Value" field from an ErrorLog row's free-text ErrorContext column.</summary>
public sealed record ContextDetailField(string Key, string Value);

/// <summary>A single deadlock event parsed from the "system_health" XE session's ring buffer.</summary>
public sealed class DeadlockEventDto
{
    /// <summary>Gets the event's timestamp.</summary>
    public DateTime EventTimeUtc { get; init; }

    /// <summary>Gets the XE process id of the process chosen as deadlock victim.</summary>
    public string? VictimProcessId { get; init; }

    /// <summary>Gets the number of processes involved in the deadlock cycle.</summary>
    public int ParticipantCount { get; init; }

    /// <summary>Gets a human-readable summary of the contended resource(s).</summary>
    public string ResourceSummary { get; init; } = string.Empty;

    /// <summary>Gets the full &lt;deadlock&gt; XML fragment, for drill-down/export.</summary>
    public string RawXml { get; init; } = string.Empty;
}
