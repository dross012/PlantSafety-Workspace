// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.SystemHealth
//  Layer:          Infrastructure
//  File:           SystemHealthDapperRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Implementation of ISystemHealthRepository.
//                    - DB version: SELECT SERVERPROPERTY(...) against the primary
//                      tenant-scoped connection via ISqlConnectionFactory -- this
//                      genuinely is tenant-scoped primary connection data, unlike the
//                      server-level msdb/ErrorDB/AuditDB connections used by the sibling
//                      Operations.* repositories, so it goes through the same
//                      ISqlConnectionFactory used by ordinary tenant repositories.
//                    - Task Scheduler status: shells out to schtasks.exe /Query for each
//                      known Phase 18 automation job (see SystemHealthOptions.
//                      KnownTaskSchedulerJobs), mirroring the Process.Start pattern
//                      already used by BackupDapperRepository.TriggerBackupAsync. Never
//                      throws for an individual task -- returns "Unavailable"/"Unknown"
//                      status instead, since live Task Scheduler state is only ever
//                      available when actually running on the box with those tasks
//                      registered. SqlException handling for the DB version query routes
//                      through the shared ErrorClassifier. Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.SystemHealth.Models;
using PlantSafety.Application.Operations.SystemHealth.Repositories;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Operations.SystemHealth.Options;
using PlantSafety.Infrastructure.Persistence.ConnectionFactory;

namespace PlantSafety.Infrastructure.Operations.SystemHealth.Persistence;

/// <summary>Dapper-backed implementation of ISystemHealthRepository.</summary>
public sealed class SystemHealthDapperRepository : ISystemHealthRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly SystemHealthOptions _options;
    private readonly ILogger<SystemHealthDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SystemHealthDapperRepository(
        ISqlConnectionFactory connectionFactory,
        IOptions<SystemHealthOptions> options,
        ILogger<SystemHealthDapperRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SystemHealthDatabaseVersionDto> GetDatabaseVersionAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(128))  AS ProductVersion,
    CAST(SERVERPROPERTY('ProductLevel')   AS NVARCHAR(128))  AS ProductLevel,
    CAST(SERVERPROPERTY('Edition')        AS NVARCHAR(128))  AS Edition,
    CAST(SERVERPROPERTY('EngineEdition')  AS INT)             AS EngineEdition;";

        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var row = await connection.QuerySingleAsync<SystemHealthDatabaseVersionDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return row;
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "SERVERPROPERTY (System Health DB version)", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskSchedulerJobStatusDto>> GetTaskSchedulerStatusAsync(CancellationToken cancellationToken)
    {
        var results = new List<TaskSchedulerJobStatusDto>(_options.KnownTaskSchedulerJobs.Count);

        foreach (var job in _options.KnownTaskSchedulerJobs)
        {
            results.Add(await QuerySingleTaskAsync(job, cancellationToken));
        }

        return results;
    }

    private async Task<TaskSchedulerJobStatusDto> QuerySingleTaskAsync(KnownTaskSchedulerJob job, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("/Query");
            psi.ArgumentList.Add("/TN");
            psi.ArgumentList.Add(job.TaskName);
            psi.ArgumentList.Add("/FO");
            psi.ArgumentList.Add("LIST");
            psi.ArgumentList.Add("/V");

            using var process = Process.Start(psi);
            if (process is null)
            {
                _logger.LogWarning("schtasks.exe could not be started to query Task Scheduler job {TaskName}.", job.TaskName);
                return Unavailable(job);
            }

            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            _ = process.StandardError.ReadToEndAsync();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.SchTasksTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("schtasks.exe query for Task Scheduler job {TaskName} timed out.", job.TaskName);
                TryKill(process);
                return Unavailable(job);
            }

            if (process.ExitCode != 0)
            {
                // Non-zero exit typically means the task is not registered on this machine
                // (e.g. "ERROR: The system cannot find the file specified.") -- distinct from
                // "we could not even check", so this is reported as "Unknown" rather than
                // "Unavailable".
                return new TaskSchedulerJobStatusDto(job.TaskName, job.Category, "Unknown", null, null);
            }

            var stdOut = await stdOutTask.ConfigureAwait(false);
            return ParseSchTasksOutput(job, stdOut);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to query Task Scheduler status for {TaskName} via schtasks.exe -- treating as Unavailable.",
                job.TaskName);
            return Unavailable(job);
        }
    }

    private TaskSchedulerJobStatusDto ParseSchTasksOutput(KnownTaskSchedulerJob job, string stdOut)
    {
        try
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rawLine in stdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.TrimEnd('\r');
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex <= 0) continue;

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();
                fields[key] = value;
            }

            var status = fields.GetValueOrDefault("Status", "Unknown");
            var lastResult = fields.GetValueOrDefault("Last Result");

            DateTime? lastRunTimeUtc = null;
            if (fields.TryGetValue("Last Run Time", out var lastRunRaw)
                && !string.IsNullOrWhiteSpace(lastRunRaw)
                && !lastRunRaw.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                && DateTime.TryParse(lastRunRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                lastRunTimeUtc = DateTime.SpecifyKind(parsed, DateTimeKind.Local).ToUniversalTime();
            }

            return new TaskSchedulerJobStatusDto(job.TaskName, job.Category, status, lastRunTimeUtc, lastResult);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse schtasks.exe output for Task Scheduler job {TaskName}.", job.TaskName);
            return Unavailable(job);
        }
    }

    private static TaskSchedulerJobStatusDto Unavailable(KnownTaskSchedulerJob job) =>
        new(job.TaskName, job.Category, "Unavailable", null, null);

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}
