// ============================================================================
// File: OperationsMonitoringV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller for the Operations.Monitoring diagnostics dashboard
//   (DB Health, DB Size, DB Performance, DB Errors, DB Deadlocks). Routes all requests
//   through MediatR ISender to the Application layer. No domain logic or data access here.
//
//   READ-ONLY module: every endpoint below is a GET. There is no write/trigger action of
//   any kind on this controller.
//
//   DB Health delegates to the existing HealthCheckService (see
//   GetDatabaseHealthReportQuery / IDatabaseHealthReportProvider) rather than
//   re-implementing DatabaseHealthCheck/SchemaVersionHealthCheck. DB Size delegates to the
//   existing Operations.Backup IBackupRepository (see GetMonitoringDatabaseSizeQuery)
//   rather than re-querying msdb.dbo.PlantSafety_DatabaseSizeLog a second time.
// ============================================================================
using System;
using System.Text;

using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;

namespace PlantSafety.Api.Operations.Monitoring.Controllers.V1;

/// <summary>V1 controller for the Operations.Monitoring module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/monitoring")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.Monitoring")]
public sealed class OperationsMonitoringV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsMonitoringV1Controller"/>.</summary>
    public OperationsMonitoringV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>Returns the aggregated /health/ready readiness report (DB Health).</summary>
    [HttpGet("health")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<DatabaseHealthReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDatabaseHealthReportQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<DatabaseHealthReportDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<DatabaseHealthReportDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Returns recent database size snapshots (DB Size), delegating to the existing
    /// Operations.Backup database-size history (msdb.dbo.PlantSafety_DatabaseSizeLog).
    /// </summary>
    [HttpGet("database-size")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DatabaseSizeSnapshotDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDatabaseSizeAsync(
        [FromQuery] string? databaseName,
        [FromQuery] int top = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetMonitoringDatabaseSizeQuery(databaseName, top), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<DatabaseSizeSnapshotDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<DatabaseSizeSnapshotDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Returns the top queries by average CPU time for a single database's Query Store
    /// (DB Performance).
    /// </summary>
    [HttpGet("query-performance/{databaseKey}")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<QueryPerformanceItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQueryPerformanceAsync(
        string databaseKey,
        [FromQuery] int top = 20,
        [FromQuery] int lookbackHours = 24,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetQueryPerformanceQuery(databaseKey, top, lookbackHours), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<QueryPerformanceItemDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<QueryPerformanceItemDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Returns recent rows from [PlantSafety.ErrorDB].dbo.ErrorLog, newest first (DB Errors),
    /// with optional severity-band (MinSeverity/MaxSeverity), source-database, module, and
    /// error-procedure filters.
    /// </summary>
    [HttpGet("errors")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ErrorLogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrorsAsync(
        [FromQuery] int? minSeverity,
        [FromQuery] DateTime? sinceUtc,
        [FromQuery] int top = 50,
        [FromQuery] int skip = 0,
        [FromQuery] int? maxSeverity = null,
        [FromQuery] string? sourceDatabase = null,
        [FromQuery] string? module = null,
        [FromQuery] string? errorProcedure = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetErrorLogQuery(minSeverity, sinceUtc, top, skip, maxSeverity, sourceDatabase, module, errorProcedure),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<ErrorLogItemDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<ErrorLogItemDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Exports [PlantSafety.ErrorDB].dbo.ErrorLog rows matching the given filters as a flat
    /// CSV file, built server-side from the same query used by <see cref="GetErrorsAsync"/>
    /// (capped at 5000 rows). Read-only -- never mutates ErrorLog.
    /// </summary>
    [HttpGet("errors/export")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ErrorLogItemDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportErrorsAsync(
        [FromQuery] int? minSeverity,
        [FromQuery] DateTime? sinceUtc,
        [FromQuery] int? maxSeverity = null,
        [FromQuery] string? sourceDatabase = null,
        [FromQuery] string? module = null,
        [FromQuery] string? errorProcedure = null,
        CancellationToken cancellationToken = default)
    {
        // Export intentionally requests the maximum single page (no further pagination).
        var result = await _sender.Send(
            new GetErrorLogQuery(minSeverity, sinceUtc, Top: 5000, Skip: 0, maxSeverity, sourceDatabase, module, errorProcedure),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<ErrorLogItemDto>>.Fail(result.Error.Message, CorrelationId));

        var csvBytes = Encoding.UTF8.GetBytes(BuildCsv(result.Value));
        var fileName = $"error-log-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(csvBytes, "text/csv", fileName);
    }

    /// <summary>Builds a flat, RFC-4180-quoted CSV from the given error log entries. No external library required.</summary>
    private static string BuildCsv(IReadOnlyList<ErrorLogItemDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ErrorDateUtc,ErrorNumber,ErrorSeverity,ErrorState,SourceDatabase,Module,ErrorProcedure,ErrorLine,ErrorMessage,ErrorContext,ErrorParameters");

        foreach (var item in items)
        {
            sb.Append(CsvField(item.ErrorDateUtc.ToString("O"))).Append(',');
            sb.Append(CsvField(item.ErrorNumber?.ToString())).Append(',');
            sb.Append(CsvField(item.ErrorSeverity?.ToString())).Append(',');
            sb.Append(CsvField(item.ErrorState?.ToString())).Append(',');
            sb.Append(CsvField(item.SourceDatabase)).Append(',');
            sb.Append(CsvField(item.Module)).Append(',');
            sb.Append(CsvField(item.ErrorProcedure)).Append(',');
            sb.Append(CsvField(item.ErrorLine?.ToString())).Append(',');
            sb.Append(CsvField(item.ErrorMessage)).Append(',');
            sb.Append(CsvField(item.ErrorContext)).Append(',');
            sb.Append(CsvField(item.ErrorParameters));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>Quotes and escapes a single CSV field per RFC 4180.</summary>
    private static string CsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    /// <summary>
    /// Returns the most recent deadlock events parsed from the built-in "system_health"
    /// Extended Events session's ring buffer (DB Deadlocks).
    /// </summary>
    [HttpGet("deadlocks")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeadlockEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeadlocksAsync(
        [FromQuery] int top = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetDeadlockEventsQuery(top), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<DeadlockEventDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<DeadlockEventDto>>.Ok(result.Value, CorrelationId));
    }
}
