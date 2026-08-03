// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteSimulationQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteSimulationQuery. Loads the live RouteDefinition aggregate
//                  (via the Domain write-side repository, since simulation needs each step's
//                  EstimatedDurationMinutes and domain-computed RouteStepStatus, which the flat
//                  read-model DTO does not carry) and hands its current step sequence to the pure
//                  Domain-layer RouteSimulationEngine -- this handler performs no simulation logic
//                  itself. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Domain.ProcessManagement.Routing.Services;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;

namespace PlantSafety.Application.ProcessManagement.Routing.Handlers;

/// <summary>Handles <see cref="GetRouteSimulationQuery"/>.</summary>
public sealed class GetRouteSimulationQueryHandler : IRequestHandler<GetRouteSimulationQuery, Result<RouteSimulationResultDto>>
{
    private readonly IRouteDefinitionRepository _repository;
    private readonly ILogger<GetRouteSimulationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteSimulationQueryHandler(IRouteDefinitionRepository repository, ILogger<GetRouteSimulationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteSimulationResultDto>> Handle(GetRouteSimulationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RouteDefinitionId.FromGuid(request.RouteDefinitionId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RouteSimulationResultDto>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteDefinition.NotFound",
                    "The requested route definition could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var outcome = RouteSimulationEngine.Simulate(aggregate.Steps);

            return Result<RouteSimulationResultDto>.Success(new RouteSimulationResultDto
            {
                TotalDurationMinutes = outcome.TotalDurationMinutes,
                CriticalPath = outcome.CriticalPathStepNumbers,
                ResourceConflictCount = outcome.ResourceConflictCount,
                MaterialShortageCount = outcome.MaterialShortageCount,
                BottleneckSteps = outcome.BottleneckStepNumbers,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to simulate RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteSimulationResultDto>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.SimulationFailed",
                "Unable to run the route simulation at this time.",
                Guid.NewGuid()));
        }
    }
}
