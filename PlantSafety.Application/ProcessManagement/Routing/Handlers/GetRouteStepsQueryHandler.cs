// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteStepsQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteStepsQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Application.ProcessManagement.Routing.Handlers;

/// <summary>Handles <see cref="GetRouteStepsQuery"/>.</summary>
public sealed class GetRouteStepsQueryHandler : IRequestHandler<GetRouteStepsQuery, Result<IReadOnlyList<RouteStepItemDto>>>
{
    private readonly IRouteDefinitionReadRepository _repository;
    private readonly ILogger<GetRouteStepsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteStepsQueryHandler(IRouteDefinitionReadRepository repository, ILogger<GetRouteStepsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<RouteStepItemDto>>> Handle(GetRouteStepsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var steps = await _repository.GetStepsAsync(request.TenantId, request.RouteDefinitionId, cancellationToken);

            if (steps is null)
            {
                return Result<IReadOnlyList<RouteStepItemDto>>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteDefinition.NotFound",
                    "The requested route definition could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<IReadOnlyList<RouteStepItemDto>>.Success(steps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve steps for RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<IReadOnlyList<RouteStepItemDto>>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.StepsRetrievalFailed",
                "Unable to retrieve route steps at this time.",
                Guid.NewGuid()));
        }
    }
}
