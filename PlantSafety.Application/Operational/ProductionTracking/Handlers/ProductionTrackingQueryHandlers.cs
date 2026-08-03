// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionTrackingQueryHandlers.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handlers for every "plain read" ProductionTracking query (list/summary/metadata/
//                  timeline/batches/events/history) that maps directly onto
//                  IProductionTrackingReadRepository with no additional domain-service composition.
//                  The four KPI/bottleneck queries that reuse the real Phase 5
//                  Analytics.Dashboard.Kpi.ProductionKpiEngine and
//                  Observability.Telemetry.TelemetryAnomalyDetector live separately in
//                  ProductionTrackingKpiQueryHandlers.cs. Consolidated into one file per the sibling
//                  WorkOrderQueryHandlers.cs precedent of grouping closely-related small handlers
//                  that share the identical try/catch-and-log shape. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.ProductionTracking.Models;
using PlantSafety.Application.Operational.ProductionTracking.Queries;
using PlantSafety.Application.Operational.ProductionTracking.Repositories;

namespace PlantSafety.Application.Operational.ProductionTracking.Handlers;

/// <summary>Handles <see cref="ListActiveProductionRunsQuery"/>.</summary>
public sealed class ListActiveProductionRunsQueryHandler(IProductionTrackingReadRepository repository, ILogger<ListActiveProductionRunsQueryHandler> logger)
    : IRequestHandler<ListActiveProductionRunsQuery, Result<PaginatedResult<ProductionRunRowDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<ProductionRunRowDto>>> Handle(ListActiveProductionRunsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.ListActiveRunsAsync(request.TenantId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<ProductionRunRowDto>>.Success(new PaginatedResult<ProductionRunRowDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list active ProductionRuns for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<ProductionRunRowDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.ListRunsFailed", "Unable to retrieve active production runs at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetRunSummaryQuery"/>.</summary>
public sealed class GetRunSummaryQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetRunSummaryQueryHandler> logger)
    : IRequestHandler<GetRunSummaryQuery, Result<RunSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<RunSummaryDto>> Handle(GetRunSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await repository.GetRunSummaryAsync(request.TenantId, request.RunId, cancellationToken);
            if (item is null)
            {
                return Result<RunSummaryDto>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.NotFound", "The requested production run could not be found.", request.RunId, ErrorSeverity.NotFound));
            }

            return Result<RunSummaryDto>.Success(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve run summary for ProductionRun {RunId}", request.RunId);
            return Result<RunSummaryDto>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RunSummaryFailed", "Unable to retrieve the production run summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetRunMetadataQuery"/>.</summary>
public sealed class GetRunMetadataQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetRunMetadataQueryHandler> logger)
    : IRequestHandler<GetRunMetadataQuery, Result<RunMetadataDto>>
{
    /// <inheritdoc/>
    public async Task<Result<RunMetadataDto>> Handle(GetRunMetadataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await repository.GetRunMetadataAsync(request.TenantId, request.RunId, cancellationToken);
            if (item is null)
            {
                return Result<RunMetadataDto>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.NotFound", "The requested production run could not be found.", request.RunId, ErrorSeverity.NotFound));
            }

            return Result<RunMetadataDto>.Success(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve run metadata for ProductionRun {RunId}", request.RunId);
            return Result<RunMetadataDto>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RunMetadataFailed", "Unable to retrieve the production run metadata at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetRunTimelineQuery"/>.</summary>
public sealed class GetRunTimelineQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetRunTimelineQueryHandler> logger)
    : IRequestHandler<GetRunTimelineQuery, Result<PaginatedResult<TimelineEventDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<TimelineEventDto>>> Handle(GetRunTimelineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetRunTimelineAsync(request.TenantId, request.RunId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<TimelineEventDto>>.Success(new PaginatedResult<TimelineEventDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve timeline for ProductionRun {RunId}", request.RunId);
            return Result<PaginatedResult<TimelineEventDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RunTimelineFailed", "Unable to retrieve the production run timeline at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetRunBatchesQuery"/>.</summary>
public sealed class GetRunBatchesQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetRunBatchesQueryHandler> logger)
    : IRequestHandler<GetRunBatchesQuery, Result<PaginatedResult<BatchRowDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<BatchRowDto>>> Handle(GetRunBatchesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetRunBatchesAsync(request.TenantId, request.RunId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<BatchRowDto>>.Success(new PaginatedResult<BatchRowDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve batches for ProductionRun {RunId}", request.RunId);
            return Result<PaginatedResult<BatchRowDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RunBatchesFailed", "Unable to retrieve the production run's batches at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetRunEventsQuery"/>.</summary>
public sealed class GetRunEventsQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetRunEventsQueryHandler> logger)
    : IRequestHandler<GetRunEventsQuery, Result<PaginatedResult<EventRowDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<EventRowDto>>> Handle(GetRunEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetRunEventsAsync(request.TenantId, request.RunId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<EventRowDto>>.Success(new PaginatedResult<EventRowDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve events for ProductionRun {RunId}", request.RunId);
            return Result<PaginatedResult<EventRowDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RunEventsFailed", "Unable to retrieve the production run's events at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ListProductionBatchesQuery"/>.</summary>
public sealed class ListProductionBatchesQueryHandler(IProductionTrackingReadRepository repository, ILogger<ListProductionBatchesQueryHandler> logger)
    : IRequestHandler<ListProductionBatchesQuery, Result<PaginatedResult<BatchRowDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<BatchRowDto>>> Handle(ListProductionBatchesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.ListBatchesAsync(request.TenantId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<BatchRowDto>>.Success(new PaginatedResult<BatchRowDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list batches for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<BatchRowDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.ListBatchesFailed", "Unable to retrieve production batches at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetProductionTimelineQuery"/>.</summary>
public sealed class GetProductionTimelineQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetProductionTimelineQueryHandler> logger)
    : IRequestHandler<GetProductionTimelineQuery, Result<PaginatedResult<TimelineEventDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<TimelineEventDto>>> Handle(GetProductionTimelineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetProductionTimelineAsync(request.TenantId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<TimelineEventDto>>.Success(new PaginatedResult<TimelineEventDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve plant-wide production timeline for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<TimelineEventDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.TimelineFailed", "Unable to retrieve the production timeline at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetEventHistoryQuery"/>.</summary>
public sealed class GetEventHistoryQueryHandler(IProductionTrackingReadRepository repository, ILogger<GetEventHistoryQueryHandler> logger)
    : IRequestHandler<GetEventHistoryQuery, Result<PaginatedResult<EventRowDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<EventRowDto>>> Handle(GetEventHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();

            var (items, total) = await repository.GetEventHistoryAsync(request.TenantId, pageNumber, pageSize, cancellationToken);
            return Result<PaginatedResult<EventRowDto>>.Success(new PaginatedResult<EventRowDto>(items, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve production event history for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<EventRowDto>>.Failure(new ApplicationError(
                "Operational.ProductionTracking.EventHistoryFailed", "Unable to retrieve the production event history at this time.", Guid.NewGuid()));
        }
    }
}
