// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSafetyProjectByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.PlanningProjects.Models.Projects;
using PlantSafety.Application.Features.PlanningProjects.Queries.Projects.GetSafetyProject;

namespace PlantSafety.Application.Features.PlanningProjects.Handlers.Projects;

/// <summary>Handles <see cref="GetSafetyProjectByIdQuery"/>.</summary>
public sealed class GetSafetyProjectByIdQueryHandler : IQueryHandler<GetSafetyProjectByIdQuery, SafetyProjectDto?>
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<GetSafetyProjectByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyProjectByIdQueryHandler(IProjectRepository repository, ILogger<GetSafetyProjectByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SafetyProjectDto?> Handle(GetSafetyProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.ProjectId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("SafetyProject {ProjectId} not found for Tenant {TenantId}", request.ProjectId, request.TenantId);
            return null;
        }

        return new SafetyProjectDto
        {
            ProjectId = model.ProjectId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            Name = model.Name,
            Description = model.Description,
            Status = model.Status,
            Priority = model.Priority,
            ProjectOwnerId = model.ProjectOwnerId,
            ProjectManagerId = model.ProjectManagerId,
            PlannedStartUtc = model.PlannedStartUtc,
            PlannedEndUtc = model.PlannedEndUtc,
            ActualStartUtc = model.ActualStartUtc,
            ActualEndUtc = model.ActualEndUtc,
            PlannedBudget = model.PlannedBudget,
            OpenMilestoneCount = model.OpenMilestoneCount,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}
