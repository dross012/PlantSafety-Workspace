// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteDefinitionsQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteDefinitionsQuery. Mirrors
//                  PlantSafety.Application.Analytics.Reporting.Handlers.GetReportsPagedQueryHandler.
//                  Additive-only evolution.
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

/// <summary>Handles <see cref="GetRouteDefinitionsQuery"/>.</summary>
public sealed class GetRouteDefinitionsQueryHandler : IRequestHandler<GetRouteDefinitionsQuery, Result<PaginatedResult<RouteDefinitionSummaryDto>>>
{
    private readonly IRouteDefinitionReadRepository _repository;
    private readonly ILogger<GetRouteDefinitionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteDefinitionsQueryHandler(IRouteDefinitionReadRepository repository, ILogger<GetRouteDefinitionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<RouteDefinitionSummaryDto>>> Handle(GetRouteDefinitionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber < 1 ? PaginationConstants.DefaultPageNumber : request.PageNumber;
            var pageSize = request.PageSize < PaginationConstants.MinPageSize
                ? PaginationConstants.DefaultPageSize
                : Math.Min(request.PageSize, PaginationConstants.MaxPageSize);

            var (items, totalCount) = await _repository.GetPagedAsync(request.TenantId, request.Search, pageNumber, pageSize, cancellationToken);

            return Result<PaginatedResult<RouteDefinitionSummaryDto>>.Success(new PaginatedResult<RouteDefinitionSummaryDto>(items, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve paginated route definitions for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<RouteDefinitionSummaryDto>>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.ListRetrievalFailed", "Unable to retrieve the route catalog at this time.", Guid.NewGuid()));
        }
    }
}
