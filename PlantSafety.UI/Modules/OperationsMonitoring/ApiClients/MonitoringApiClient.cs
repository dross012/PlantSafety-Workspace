// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Typed API client for the Operations.Monitoring module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.Monitoring.Controllers.V1
//   .OperationsMonitoringV1Controller. READ-ONLY — every method here is a GET.
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
using PlantSafety.UI.Modules.OperationsMonitoring.Models;

namespace PlantSafety.UI.Modules.OperationsMonitoring.ApiClients;

/// <summary>Typed API client for the Operations.Monitoring module.</summary>
public sealed class MonitoringApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="MonitoringApiClient"/>.</summary>
    public MonitoringApiClient(HttpClient http, ILogger<MonitoringApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the aggregated /health/ready readiness report (DB Health).</summary>
    public Task<ApiResponseEnvelope<DatabaseHealthReportDto>> GetHealthAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<DatabaseHealthReportDto>("v1/operations/monitoring/health", cancellationToken);

    /// <summary>Retrieves recent database size snapshots (DB Size).</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<DatabaseSizeSnapshotDto>>> GetDatabaseSizeAsync(
        string? databaseName = null,
        int top = 100,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<DatabaseSizeSnapshotDto>>(
            $"v1/operations/monitoring/database-size?top={top}" + (string.IsNullOrWhiteSpace(databaseName) ? string.Empty : $"&databaseName={Uri.EscapeDataString(databaseName)}"),
            cancellationToken);

    /// <summary>Retrieves the top queries by average CPU time for a single database's Query Store (DB Performance).</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<QueryPerformanceItemDto>>> GetQueryPerformanceAsync(
        string databaseKey,
        int top = 20,
        int lookbackHours = 24,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<QueryPerformanceItemDto>>(
            $"v1/operations/monitoring/query-performance/{Uri.EscapeDataString(databaseKey)}?top={top}&lookbackHours={lookbackHours}",
            cancellationToken);

    /// <summary>
    /// Retrieves recent rows from [PlantSafety.ErrorDB].dbo.ErrorLog, newest first (DB Errors),
    /// with optional severity-band (minSeverity/maxSeverity), source-database, module, and
    /// error-procedure filters.
    /// </summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<ErrorLogItemDto>>> GetErrorsAsync(
        int? minSeverity = null,
        DateTime? sinceUtc = null,
        int top = 50,
        int skip = 0,
        int? maxSeverity = null,
        string? sourceDatabase = null,
        string? module = null,
        string? errorProcedure = null,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<ErrorLogItemDto>>(
            BuildErrorLogQueryString("v1/operations/monitoring/errors", minSeverity, sinceUtc, maxSeverity, sourceDatabase, module, errorProcedure, top, skip),
            cancellationToken);

    /// <summary>
    /// Downloads a CSV export of ErrorLog rows matching the given filters, built server-side
    /// from the same query used by <see cref="GetErrorsAsync"/>. Returns the raw CSV bytes for
    /// the caller to trigger a browser download for -- this client never writes to disk itself.
    /// </summary>
    public async Task<byte[]?> DownloadErrorLogExportCsvAsync(
        int? minSeverity = null,
        DateTime? sinceUtc = null,
        int? maxSeverity = null,
        string? sourceDatabase = null,
        string? module = null,
        string? errorProcedure = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildErrorLogQueryString(
            "v1/operations/monitoring/errors/export", minSeverity, sinceUtc, maxSeverity, sourceDatabase, module, errorProcedure, top: null, skip: null);

        var response = await Http.GetAsync(query, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string BuildErrorLogQueryString(
        string relativePath,
        int? minSeverity,
        DateTime? sinceUtc,
        int? maxSeverity,
        string? sourceDatabase,
        string? module,
        string? errorProcedure,
        int? top,
        int? skip)
    {
        var query = relativePath + "?";
        if (top is not null) query += $"top={top}&";
        if (skip is not null) query += $"skip={skip}&";
        if (minSeverity is not null) query += $"minSeverity={minSeverity}&";
        if (maxSeverity is not null) query += $"maxSeverity={maxSeverity}&";
        if (sinceUtc is not null) query += $"sinceUtc={Uri.EscapeDataString(sinceUtc.Value.ToString("O"))}&";
        if (!string.IsNullOrWhiteSpace(sourceDatabase)) query += $"sourceDatabase={Uri.EscapeDataString(sourceDatabase)}&";
        if (!string.IsNullOrWhiteSpace(module)) query += $"module={Uri.EscapeDataString(module)}&";
        if (!string.IsNullOrWhiteSpace(errorProcedure)) query += $"errorProcedure={Uri.EscapeDataString(errorProcedure)}&";

        return query.TrimEnd('&', '?');
    }

    /// <summary>Retrieves the most recent deadlock events (DB Deadlocks).</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<DeadlockEventDto>>> GetDeadlocksAsync(
        int top = 20,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<DeadlockEventDto>>($"v1/operations/monitoring/deadlocks?top={top}", cancellationToken);
}
