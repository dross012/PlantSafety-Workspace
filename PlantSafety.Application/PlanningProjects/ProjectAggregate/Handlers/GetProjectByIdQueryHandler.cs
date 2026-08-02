// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           GetProjectByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetProjectByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Handlers
//                  .GetRootCauseAnalysisByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Models;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Queries;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Repositories;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Handlers;

/// <summary>Handles <see cref="GetProjectByIdQuery"/>.</summary>
public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDetailDto>>
{
    private readonly IProjectReadRepository _repository;
    private readonly ILogger<GetProjectByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProjectByIdQueryHandler(IProjectReadRepository repository, ILogger<GetProjectByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProjectDetailDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ProjectId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Project {ProjectId} not found for Tenant {TenantId}", request.ProjectId, request.TenantId);

                return Result<ProjectDetailDto>.Failure(new ApplicationError(
                    "PlanningProjects.Project.NotFound",
                    "The requested project could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ProjectDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Project {ProjectId}", request.ProjectId);

            return Result<ProjectDetailDto>.Failure(new ApplicationError(
                "PlanningProjects.Project.RetrievalFailed",
                "Unable to retrieve project data at this time.",
                Guid.NewGuid()));
        }
    }
}
