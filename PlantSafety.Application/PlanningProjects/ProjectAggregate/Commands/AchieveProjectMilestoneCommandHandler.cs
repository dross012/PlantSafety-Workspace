// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           AchieveProjectMilestoneCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for AchieveProjectMilestoneCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PlanningProjects.Exceptions;
using PlantSafety.Domain.PlanningProjects.Interfaces;
using PlantSafety.Domain.PlanningProjects.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

/// <summary>Handles <see cref="AchieveProjectMilestoneCommand"/>.</summary>
public sealed class AchieveProjectMilestoneCommandHandler : IRequestHandler<AchieveProjectMilestoneCommand, Result<ProjectStatusResult>>
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<AchieveProjectMilestoneCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AchieveProjectMilestoneCommandHandler(IProjectRepository repository, ILogger<AchieveProjectMilestoneCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProjectStatusResult>> Handle(AchieveProjectMilestoneCommand request, CancellationToken cancellationToken)
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

            var achievedBy = UserId.FromGuid(request.AchievedByUserId);

            aggregate.AchieveMilestone(
                milestoneId: request.MilestoneId,
                actor: PerformedByActor.User(achievedBy),
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
        catch (ProjectMilestoneNotFoundException ex)
        {
            return Result<ProjectStatusResult>.Failure(new ApplicationError(
                "PlanningProjects.Project.MilestoneNotFound",
                ex.Message,
                request.ProjectId,
                ErrorSeverity.NotFound));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to achieve milestone {MilestoneId} on Project {ProjectId}", request.MilestoneId, request.ProjectId);

            return Result<ProjectStatusResult>.Failure(new ApplicationError(
                "PlanningProjects.Project.AchieveMilestoneFailed",
                "Unable to achieve the milestone at this time.",
                Guid.NewGuid()));
        }
    }
}
