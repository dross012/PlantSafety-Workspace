// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Typed API client for the DiagnosticsViewer module.
//   Fetches materialized DiagnosticResult collections from the SafetyAnalyzer
//   API endpoints. All HTTP concerns are encapsulated here — state containers
//   and components never call HttpClient directly.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.DiagnosticsViewer.Models;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.Services;

/// <summary>
/// Typed API client for the DiagnosticsViewer module.
/// All methods return <see cref="ApiResponseEnvelope{T}"/> — callers MUST
/// check <c>IsSuccess</c> before accessing <c>Data</c>.
/// </summary>
public sealed class DiagnosticsApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="DiagnosticsApiClient"/>.
    /// </summary>
    /// <param name="http">The typed HttpClient with correlation and auth handlers.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public DiagnosticsApiClient(HttpClient http, ILogger<DiagnosticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Fetches the latest diagnostic run results for the authenticated tenant.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>
    /// An envelope containing the list of <see cref="DiagnosticResult"/> items,
    /// or a problem/fault envelope on failure.
    /// </returns>
    public Task<ApiResponseEnvelope<IReadOnlyList<DiagnosticResult>>> GetLatestResultsAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<DiagnosticResult>>("v1/diagnostics/results/latest", cancellationToken);

    /// <summary>
    /// Fetches diagnostic run results by run correlation ID.
    /// </summary>
    /// <param name="runCorrelationId">The correlation ID of the diagnostic run.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The diagnostic results for the specified run.</returns>
    public Task<ApiResponseEnvelope<IReadOnlyList<DiagnosticResult>>> GetResultsByRunAsync(
        string runCorrelationId,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<DiagnosticResult>>($"v1/diagnostics/results/{runCorrelationId}", cancellationToken);

    /// <summary>
    /// Fetches the paginated list of available diagnostic run summaries.
    /// </summary>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>A paginated list of diagnostic run summaries.</returns>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<DiagnosticRunSummary>>> GetRunSummariesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<DiagnosticRunSummary>>(
            $"v1/diagnostics/runs?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);
}

/// <summary>
/// Summary descriptor for a completed diagnostic run, used in the run history list.
/// </summary>
public sealed class DiagnosticRunSummary
{
    /// <summary>Gets the correlation ID of this diagnostic run.</summary>
    public string RunCorrelationId { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the run started.</summary>
    public System.DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>Gets the total number of errors found in this run.</summary>
    public int ErrorCount { get; init; }

    /// <summary>Gets the total number of warnings found in this run.</summary>
    public int WarningCount { get; init; }

    /// <summary>Gets the total number of informational findings in this run.</summary>
    public int InformationCount { get; init; }
}
