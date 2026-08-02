// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           ResumeProjectCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ResumeProjectCommand.
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

/// <summary>Handles <see cref="ResumeProjectCommand"/>.</summary>
public sealed class ResumeProjectCommandHandler : IRequestHandler<ResumeProjectCommand, Result<ProjectStatusResult>>
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<ResumeProjectCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResumeProjectCommandHandler(IProjectRepository repository, ILogger<ResumeProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProjectStatusResult>> Handle(ResumeProjectCommand request, CancellationToken cancellationToken)
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

            var resumedBy = UserId.FromGuid(request.ResumedByUserId);

            aggregate.Resume(
                resumedBy: resumedBy,
                resumedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(resumedBy),
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
            _logger.LogError(ex, "Failed to resume Project {ProjectId}", request.ProjectId);

            return Result<ProjectStatusResult>.Failure(new ApplicationError(
                "PlanningProjects.Project.ResumeFailed",
                "Unable to resume the project at this time.",
                Guid.NewGuid()));
        }
    }
}
