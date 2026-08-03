// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           ReviseRouteDefinitionCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for ReviseRouteDefinitionCommand (same-status action; Active remains
//                  Active, Version increments, ordered step list replaced). Additively captures a
//                  new, immediately-Approved RouteVersionSnapshot for the revised content --
//                  genuinely new relative to ReviseProcessDefinitionCommandHandler, which has no
//                  structural content to snapshot. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Routing.Aggregates;
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Handles <see cref="ReviseRouteDefinitionCommand"/>.</summary>
public sealed class ReviseRouteDefinitionCommandHandler : IRequestHandler<ReviseRouteDefinitionCommand, Result<RouteDefinitionStatusResult>>
{
    private readonly IRouteDefinitionRepository _repository;
    private readonly IRouteVersionSnapshotRepository _snapshotRepository;
    private readonly ILogger<ReviseRouteDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReviseRouteDefinitionCommandHandler(
        IRouteDefinitionRepository repository,
        IRouteVersionSnapshotRepository snapshotRepository,
        ILogger<ReviseRouteDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteDefinitionStatusResult>> Handle(ReviseRouteDefinitionCommand request, CancellationToken cancellationToken)
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

            var revisedBy = UserId.FromGuid(request.RevisedByUserId);
            var now = UtcTimestamp.Now();

            var newSteps = request.Steps
                .Select(s => (s.ExistingStepId, s.Name, s.Instructions, s.Resources, s.EstimatedDurationMinutes))
                .ToList();

            aggregate.Revise(
                newSteps: newSteps,
                revisedBy: revisedBy,
                revisedAtUtc: now,
                actor: PerformedByActor.User(revisedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            var snapshot = RouteVersionSnapshot.Capture(
                aggregate.RouteDefinitionId, aggregate.TenantId, aggregate.Version,
                aggregate.ToStepSnapshots(), RouteVersionStatus.Approved,
                now, now);

            await _snapshotRepository.AddAsync(snapshot, cancellationToken);

            return Result<RouteDefinitionStatusResult>.Success(new RouteDefinitionStatusResult(
                aggregate.RouteDefinitionId.Value, aggregate.Status.ToString(), aggregate.Version));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revise RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.ReviseFailed",
                "Unable to revise the route definition at this time.",
                Guid.NewGuid()));
        }
    }
}
