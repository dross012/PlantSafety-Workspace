// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           ArchiveRouteDefinitionCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveRouteDefinitionCommand. Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.ArchiveProcessDefinitionCommandHandler.
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

/// <summary>Handles <see cref="ArchiveRouteDefinitionCommand"/>.</summary>
public sealed class ArchiveRouteDefinitionCommandHandler : IRequestHandler<ArchiveRouteDefinitionCommand, Result<RouteDefinitionStatusResult>>
{
    private readonly IRouteDefinitionRepository _repository;
    private readonly ILogger<ArchiveRouteDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveRouteDefinitionCommandHandler(IRouteDefinitionRepository repository, ILogger<ArchiveRouteDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteDefinitionStatusResult>> Handle(ArchiveRouteDefinitionCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
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
            _logger.LogError(ex, "Failed to archive RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.ArchiveFailed",
                "Unable to archive the route definition at this time.",
                Guid.NewGuid()));
        }
    }
}
