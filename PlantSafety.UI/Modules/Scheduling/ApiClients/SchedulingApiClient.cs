// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Scheduling module (Volume S1).
//   Wraps all HTTP communication for the Dashboard, Job Detail, and
//   Calendar pages. All methods return ApiResponseEnvelope<T>. No HttpClient
//   calls are permitted outside this class for the Scheduling module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Scheduling.Models;

namespace PlantSafety.UI.Modules.Scheduling.ApiClients;

/// <summary>
/// Typed API client for the Scheduling module. Components MUST inject this
/// class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class SchedulingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="SchedulingApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public SchedulingApiClient(HttpClient http, ILogger<SchedulingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the Job Summary card counts.</summary>
    public Task<ApiResponseEnvelope<JobSummaryDto>> GetJobSummaryAsync(CancellationToken cancellationToken = default)
        => GetAsync<JobSummaryDto>("v1/scheduling/jobs/summary", cancellationToken);

    /// <summary>Retrieves the Resource Summary card counts.</summary>
    public Task<ApiResponseEnvelope<ResourceSummaryDto>> GetResourceSummaryAsync(CancellationToken cancellationToken = default)
        => GetAsync<ResourceSummaryDto>("v1/scheduling/resources/summary", cancellationToken);

    /// <summary>Retrieves a paginated page of upcoming jobs for the dashboard table.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="statusFilter">Optional status filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<UpcomingJobDto>>> GetUpcomingJobsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<UpcomingJobDto>>(
            $"v1/scheduling/jobs?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(statusFilter) ? string.Empty : $"&status={Uri.EscapeDataString(statusFilter)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single job.</summary>
    public Task<ApiResponseEnvelope<JobDetailDto>> GetJobDetailAsync(Guid jobId, CancellationToken cancellationToken = default)
        => GetAsync<JobDetailDto>($"v1/scheduling/jobs/{jobId}", cancellationToken);

    /// <summary>Retrieves the ordered timeline steps for a job.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<TimelineStepDto>>> GetTimelineAsync(Guid jobId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<TimelineStepDto>>($"v1/scheduling/jobs/{jobId}/timeline", cancellationToken);

    /// <summary>Retrieves the scheduling constraints evaluated for a job.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<ConstraintDto>>> GetConstraintsAsync(Guid jobId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<ConstraintDto>>($"v1/scheduling/jobs/{jobId}/constraints", cancellationToken);

    /// <summary>Retrieves fresh predictive scheduling insights for a job (never cached).</summary>
    public Task<ApiResponseEnvelope<PredictiveInsightsDto>> GetPredictiveInsightsAsync(Guid jobId, CancellationToken cancellationToken = default)
        => GetAsync<PredictiveInsightsDto>($"v1/scheduling/jobs/{jobId}/predictive", cancellationToken);

    /// <summary>Retrieves the calendar week grid starting at <paramref name="weekStart"/>.</summary>
    public Task<ApiResponseEnvelope<CalendarWeekDto>> GetCalendarWeekAsync(DateOnly weekStart, CancellationToken cancellationToken = default)
        => GetAsync<CalendarWeekDto>($"v1/scheduling/calendar?weekStart={weekStart:yyyy-MM-dd}", cancellationToken);
}
