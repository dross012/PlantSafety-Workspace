// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteDefinitionByIdQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteDefinitionByIdQuery. Mirrors
//                  PlantSafety.Application.ProcessManagement.Handlers.GetProcessDefinitionByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Application.ProcessManagement.Routing.Handlers;

/// <summary>Handles <see cref="GetRouteDefinitionByIdQuery"/>.</summary>
public sealed class GetRouteDefinitionByIdQueryHandler : IRequestHandler<GetRouteDefinitionByIdQuery, Result<RouteDefinitionDetailDto>>
{
    private readonly IRouteDefinitionReadRepository _repository;
    private readonly ILogger<GetRouteDefinitionByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteDefinitionByIdQueryHandler(IRouteDefinitionReadRepository repository, ILogger<GetRouteDefinitionByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteDefinitionDetailDto>> Handle(GetRouteDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.TenantId, request.RouteDefinitionId, cancellationToken);

            if (item is null)
            {
                _logger.LogDebug("RouteDefinition {RouteDefinitionId} not found for Tenant {TenantId}", request.RouteDefinitionId, request.TenantId);

                return Result<RouteDefinitionDetailDto>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteDefinition.NotFound",
                    "The requested route definition could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<RouteDefinitionDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteDefinitionDetailDto>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.RetrievalFailed",
                "Unable to retrieve route definition data at this time.",
                Guid.NewGuid()));
        }
    }
}
