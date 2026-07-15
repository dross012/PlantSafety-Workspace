// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Production Tracking module
//   (Volume T1). Wraps all HTTP communication for the Dashboard, Run
//   Detail, Batch Tracking, Timeline, KPI Dashboard, and Event History
//   pages. All methods return ApiResponseEnvelope<T>.
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
using PlantSafety.UI.Modules.ProductionTracking.Models;

namespace PlantSafety.UI.Modules.ProductionTracking.ApiClients;

/// <summary>Typed API client for the Production Tracking module.</summary>
public sealed class ProductionTrackingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ProductionTrackingApiClient"/>.</summary>
    public ProductionTrackingApiClient(HttpClient http, ILogger<ProductionTrackingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the Throughput Summary card.</summary>
    public Task<ApiResponseEnvelope<ThroughputSummaryDto>> GetThroughputAsync(CancellationToken cancellationToken = default)
        => GetAsync<ThroughputSummaryDto>("v1/production-tracking/throughput", cancellationToken);

    /// <summary>Retrieves the Bottleneck Summary card.</summary>
    public Task<ApiResponseEnvelope<BottleneckSummaryDto>> GetBottlenecksAsync(CancellationToken cancellationToken = default)
        => GetAsync<BottleneckSummaryDto>("v1/production-tracking/bottlenecks", cancellationToken);

    /// <summary>Retrieves a paginated page of active production runs.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<ProductionRunRowDto>>> GetActiveRunsAsync(
        int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<ProductionRunRowDto>>(
            $"v1/production-tracking/runs?pageNumber={pageNumber}&pageSize={pageSize}&activeOnly=true", cancellationToken);

    /// <summary>Retrieves the Run Summary panel for a single run.</summary>
    public Task<ApiResponseEnvelope<RunSummaryDto>> GetRunSummaryAsync(Guid runId, CancellationToken cancellationToken = default)
        => GetAsync<RunSummaryDto>($"v1/production-tracking/runs/{runId}", cancellationToken);

    /// <summary>Retrieves the Metadata panel for a single run.</summary>
    public Task<ApiResponseEnvelope<RunMetadataDto>> GetRunMetadataAsync(Guid runId, CancellationToken cancellationToken = default)
        => GetAsync<RunMetadataDto>($"v1/production-tracking/runs/{runId}/metadata", cancellationToken);

    /// <summary>Retrieves the chronological timeline for a single run.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<TimelineEventDto>>> GetRunTimelineAsync(Guid runId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<TimelineEventDto>>($"v1/production-tracking/runs/{runId}/timeline", cancellationToken);

    /// <summary>Retrieves the batches produced by a single run.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<BatchRowDto>>> GetRunBatchesAsync(Guid runId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<BatchRowDto>>($"v1/production-tracking/runs/{runId}/batches", cancellationToken);

    /// <summary>Retrieves the KPI tiles for a single run.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<KpiTileDto>>> GetRunKpisAsync(Guid runId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<KpiTileDto>>($"v1/production-tracking/runs/{runId}/kpis", cancellationToken);

    /// <summary>Retrieves the event log for a single run.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<EventRowDto>>> GetRunEventsAsync(Guid runId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<EventRowDto>>($"v1/production-tracking/runs/{runId}/events", cancellationToken);

    /// <summary>Retrieves a paginated page of batches across all runs.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<BatchRowDto>>> GetBatchesAsync(
        int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<BatchRowDto>>(
            $"v1/production-tracking/batches?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves plant-wide KPI tiles for the KPI Dashboard page.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<KpiTileDto>>> GetKpisAsync(CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<KpiTileDto>>("v1/production-tracking/kpis", cancellationToken);

    /// <summary>Retrieves the plant-wide chronological production timeline.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<TimelineEventDto>>> GetTimelineAsync(
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<TimelineEventDto>>(
            $"v1/production-tracking/timeline?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves the searchable/filterable production event history log.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<EventRowDto>>> GetEventHistoryAsync(
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<EventRowDto>>(
            $"v1/production-tracking/events?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);
}
