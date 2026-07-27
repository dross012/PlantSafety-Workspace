// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSystemHealthReportQuery. Fans out in parallel to every
//   other Operations.* module's existing MediatR queries (via ISender) plus the new
//   ISystemHealthRepository (DB version, Task Scheduler status) and a pure Assembly read
//   (app version). Each section is independently resilient: a failing sub-query is logged
//   and surfaced as an unavailable section rather than failing the whole report -- this is
//   a monitoring rollup, so one broken data source must degrade gracefully.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Queries;
using PlantSafety.Application.Operations.AuditViewer.Queries;
using Shared.AuditLogging.Abstractions.Models;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Queries;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;
using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SchemaGuard.Queries;
using PlantSafety.Application.Operations.SystemHealth.Models;
using PlantSafety.Application.Operations.SystemHealth.Queries;
using PlantSafety.Application.Operations.SystemHealth.Repositories;

namespace PlantSafety.Application.Operations.SystemHealth.Handlers;

/// <summary>Handles <see cref="GetSystemHealthReportQuery"/>.</summary>
public sealed class GetSystemHealthReportQueryHandler : IRequestHandler<GetSystemHealthReportQuery, Result<SystemHealthReportDto>>
{
    /// <summary>ErrorSeverity threshold above which an ErrorLog row counts as "high severity" (severe/system per SQL Server's documented bands).</summary>
    private const int HighSeverityThreshold = 17;

    /// <summary>The primary tenant database's Query Store key, used for the DB-performance half of the monitoring summary.</summary>
    private const string PrimaryDatabaseKey = "CoreDB";

    private readonly ISender _sender;
    private readonly ISystemHealthRepository _repository;
    private readonly ILogger<GetSystemHealthReportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSystemHealthReportQueryHandler(
        ISender sender,
        ISystemHealthRepository repository,
        ILogger<GetSystemHealthReportQueryHandler> logger)
    {
        _sender = sender;
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SystemHealthReportDto>> Handle(GetSystemHealthReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var backupTask = RunResultSectionAsync(
                "BackupHealth", () => _sender.Send(new GetBackupHealthQuery(), cancellationToken));

            var schemaTask = RunResultSectionAsync(
                "SchemaHealth", () => _sender.Send(new GetSchemaHealthQuery(), cancellationToken));

            var databaseHealthTask = RunResultSectionAsync(
                "DatabaseHealth", () => _sender.Send(new GetDatabaseHealthReportQuery(), cancellationToken));

            var monitoringSummaryTask = BuildMonitoringSummaryAsync(cancellationToken);

            var purgeStatusTask = RunResultSectionAsync(
                "PurgeStatus", () => _sender.Send(new GetPurgeStatusQuery(Zone: null), cancellationToken));

            var auditActivityTask = BuildRecentAuditActivityAsync(cancellationToken);

            var featureFlagsTask = RunResultSectionAsync(
                "FeatureFlags", () => _sender.Send(new GetAllFeatureFlagsQuery(), cancellationToken));

            var taskSchedulerTask = RunRepositorySectionAsync(
                "TaskSchedulerJobs", () => _repository.GetTaskSchedulerStatusAsync(cancellationToken));

            var databaseVersionTask = RunRepositorySectionAsync(
                "DatabaseVersion", () => _repository.GetDatabaseVersionAsync(cancellationToken));

            await Task.WhenAll(
                backupTask,
                schemaTask,
                databaseHealthTask,
                monitoringSummaryTask,
                purgeStatusTask,
                auditActivityTask,
                featureFlagsTask,
                taskSchedulerTask,
                databaseVersionTask);

            var report = new SystemHealthReportDto(
                DateTime.UtcNow,
                GetAppVersion(),
                backupTask.Result,
                schemaTask.Result,
                databaseHealthTask.Result,
                monitoringSummaryTask.Result,
                purgeStatusTask.Result,
                auditActivityTask.Result,
                featureFlagsTask.Result,
                taskSchedulerTask.Result,
                databaseVersionTask.Result);

            return Result<SystemHealthReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            // Should be unreachable -- every individual section above already catches its
            // own failures. Retained as a last-resort guard so an unexpected failure in the
            // fan-out/assembly logic itself still returns a Result rather than throwing.
            _logger.LogError(ex, "Failed to assemble the System Health rollup report.");

            return Result<SystemHealthReportDto>.Failure(new ApplicationError(
                "Operations.SystemHealth.ReportAssemblyFailed",
                "Unable to assemble the System Health report at this time.",
                Guid.NewGuid()));
        }
    }

    /// <summary>
    /// Combines Operations.Monitoring's error log (last 24h) and Query Store (primary
    /// database) queries into a single DB-performance/error-count summary section. Both
    /// sub-queries must succeed for the section to be marked available.
    /// </summary>
    private async Task<SystemHealthSectionDto<MonitoringSummaryDto>> BuildMonitoringSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sinceUtc = DateTime.UtcNow.AddHours(-24);

            var errorsTask = _sender.Send(new GetErrorLogQuery(MinSeverity: null, SinceUtc: sinceUtc, Top: 25), cancellationToken);
            var performanceTask = _sender.Send(new GetQueryPerformanceQuery(PrimaryDatabaseKey, Top: 5), cancellationToken);

            await Task.WhenAll(errorsTask, performanceTask);

            var errorsResult = errorsTask.Result;
            var performanceResult = performanceTask.Result;

            if (errorsResult.IsFailure)
            {
                _logger.LogWarning(
                    "System Health Report: MonitoringSummary section unavailable -- error log query failed: {Reason}",
                    errorsResult.Error.Message);
                return SystemHealthSectionDto<MonitoringSummaryDto>.Unavailable(errorsResult.Error.Message);
            }

            if (performanceResult.IsFailure)
            {
                _logger.LogWarning(
                    "System Health Report: MonitoringSummary section unavailable -- query performance query failed: {Reason}",
                    performanceResult.Error.Message);
                return SystemHealthSectionDto<MonitoringSummaryDto>.Unavailable(performanceResult.Error.Message);
            }

            var errors = errorsResult.Value;
            var highSeverityCount = errors.Count(e => (e.ErrorSeverity ?? 0) >= HighSeverityThreshold);

            var summary = new MonitoringSummaryDto(errors.Count, highSeverityCount, errors, performanceResult.Value);
            return SystemHealthSectionDto<MonitoringSummaryDto>.Available(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System Health Report: MonitoringSummary section failed unexpectedly.");
            return SystemHealthSectionDto<MonitoringSummaryDto>.Unavailable("Monitoring summary is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Retrieves the count of AuditLog rows written in the last 24 hours via
    /// Operations.AuditViewer's SearchAuditLogQuery (PageSize=1 -- only TotalCount is needed).
    /// </summary>
    private async Task<SystemHealthSectionDto<int>> BuildRecentAuditActivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            var criteria = new AuditLogSearchCriteria(FromUtc: DateTime.UtcNow.AddHours(-24), PageSize: 1);
            var result = await _sender.Send(new SearchAuditLogQuery(criteria), cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "System Health Report: RecentAuditActivityCount section unavailable: {Reason}", result.Error.Message);
                return SystemHealthSectionDto<int>.Unavailable(result.Error.Message);
            }

            return SystemHealthSectionDto<int>.Available(result.Value.TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System Health Report: RecentAuditActivityCount section failed unexpectedly.");
            return SystemHealthSectionDto<int>.Unavailable("Recent audit activity is temporarily unavailable.");
        }
    }

    /// <summary>Runs a single ISender.Send call, mapping its Result&lt;T&gt; outcome into a resilient section.</summary>
    private async Task<SystemHealthSectionDto<T>> RunResultSectionAsync<T>(string sectionName, Func<Task<Result<T>>> query)
    {
        try
        {
            var result = await query();

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "System Health Report: {Section} section unavailable: {Reason}", sectionName, result.Error.Message);
                return SystemHealthSectionDto<T>.Unavailable(result.Error.Message);
            }

            return SystemHealthSectionDto<T>.Available(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System Health Report: {Section} section failed unexpectedly.", sectionName);
            return SystemHealthSectionDto<T>.Unavailable($"{sectionName} is temporarily unavailable.");
        }
    }

    /// <summary>Runs a single repository call (not wrapped in Result&lt;T&gt;), mapping exceptions into a resilient section.</summary>
    private async Task<SystemHealthSectionDto<T>> RunRepositorySectionAsync<T>(string sectionName, Func<Task<T>> action)
    {
        try
        {
            var value = await action();
            return SystemHealthSectionDto<T>.Available(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System Health Report: {Section} section failed unexpectedly.", sectionName);
            return SystemHealthSectionDto<T>.Unavailable($"{sectionName} is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Reads the running application's assembly version. Prefers the entry assembly
    /// (PlantSafety.Api when hosted normally) and falls back to this assembly when no
    /// entry assembly is available (e.g. under a test host).
    /// </summary>
    private static string GetAppVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString() ?? "Unknown";
    }
}
