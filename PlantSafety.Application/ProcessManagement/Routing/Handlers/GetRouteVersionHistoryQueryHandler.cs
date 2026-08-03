// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteVersionHistoryQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteVersionHistoryQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Application.ProcessManagement.Routing.Handlers;

/// <summary>Handles <see cref="GetRouteVersionHistoryQuery"/>.</summary>
public sealed class GetRouteVersionHistoryQueryHandler : IRequestHandler<GetRouteVersionHistoryQuery, Result<PaginatedResult<RouteVersionSummaryDto>>>
{
    private readonly IRouteVersionSnapshotReadRepository _repository;
    private readonly ILogger<GetRouteVersionHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteVersionHistoryQueryHandler(IRouteVersionSnapshotReadRepository repository, ILogger<GetRouteVersionHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<RouteVersionSummaryDto>>> Handle(GetRouteVersionHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber < 1 ? PaginationConstants.DefaultPageNumber : request.PageNumber;
            var pageSize = request.PageSize < PaginationConstants.MinPageSize
                ? PaginationConstants.DefaultPageSize
                : Math.Min(request.PageSize, PaginationConstants.MaxPageSize);

            var (items, totalCount) = await _repository.GetPagedForRouteAsync(request.TenantId, request.RouteDefinitionId, pageNumber, pageSize, cancellationToken);

            return Result<PaginatedResult<RouteVersionSummaryDto>>.Success(new PaginatedResult<RouteVersionSummaryDto>(items, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve version history for RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<PaginatedResult<RouteVersionSummaryDto>>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteVersionSnapshot.HistoryRetrievalFailed",
                "Unable to retrieve the route's version history at this time.",
                Guid.NewGuid()));
        }
    }
}
