// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           CancelProjectCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CancelProjectCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PlanningProjects.Interfaces;
using PlantSafety.Domain.PlanningProjects.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

/// <summary>Handles <see cref="CancelProjectCommand"/>.</summary>
public sealed class CancelProjectCommandHandler : IRequestHandler<CancelProjectCommand, Result<ProjectStatusResult>>
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<CancelProjectCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CancelProjectCommandHandler(IProjectRepository repository, ILogger<CancelProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProjectStatusResult>> Handle(CancelProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProjectId.FromGuid(request.ProjectId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProjectStatusResult>.Failure(new ApplicationError(
                    "PlanningProjects.Project.NotFound",
                    "The requested project could not be found.",
                    request.ProjectId,
                    ErrorSeverity.NotFound));
            }

            var cancelledBy = UserId.FromGuid(request.CancelledByUserId);

            aggregate.Cancel(
                cancelledBy: cancelledBy,
                reason: request.Reason,
                cancelledAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(cancelledBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProjectStatusResult>.Success(new ProjectStatusResult(
                aggregate.ProjectId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel Project {ProjectId}", request.ProjectId);

            return Result<ProjectStatusResult>.Failure(new ApplicationError(
                "PlanningProjects.Project.CancelFailed",
                "Unable to cancel the project at this time.",
                Guid.NewGuid()));
        }
    }
}
