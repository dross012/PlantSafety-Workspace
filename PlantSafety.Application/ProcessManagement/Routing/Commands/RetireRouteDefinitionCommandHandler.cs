// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RetireRouteDefinitionCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for RetireRouteDefinitionCommand. Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.RetireProcessDefinitionCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Handles <see cref="RetireRouteDefinitionCommand"/>.</summary>
public sealed class RetireRouteDefinitionCommandHandler : IRequestHandler<RetireRouteDefinitionCommand, Result<RouteDefinitionStatusResult>>
{
    private readonly IRouteDefinitionRepository _repository;
    private readonly ILogger<RetireRouteDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireRouteDefinitionCommandHandler(IRouteDefinitionRepository repository, ILogger<RetireRouteDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteDefinitionStatusResult>> Handle(RetireRouteDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RouteDefinitionId.FromGuid(request.RouteDefinitionId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RouteDefinitionStatusResult>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteDefinition.NotFound",
                    "The requested route definition could not be found.",
                    request.RouteDefinitionId,
                    ErrorSeverity.NotFound));
            }

            var retiredBy = UserId.FromGuid(request.RetiredByUserId);

            aggregate.Retire(
                retiredBy: retiredBy,
                reason: request.Reason,
                retiredAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(retiredBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RouteDefinitionStatusResult>.Success(new RouteDefinitionStatusResult(
                aggregate.RouteDefinitionId.Value, aggregate.Status.ToString(), aggregate.Version));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retire RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.RetireFailed",
                "Unable to retire the route definition at this time.",
                Guid.NewGuid()));
        }
    }
}
