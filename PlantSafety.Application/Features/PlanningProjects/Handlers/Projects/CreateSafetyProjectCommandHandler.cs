// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateSafetyProjectCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.PlanningProjects.Commands.Projects.CreateSafetyProject;

namespace PlantSafety.Application.Features.PlanningProjects.Handlers.Projects;

/// <summary>Handles <see cref="CreateSafetyProjectCommand"/>. Returns the new ProjectId.</summary>
public sealed class CreateSafetyProjectCommandHandler : ICommandHandler<CreateSafetyProjectCommand, Guid>
{
    private readonly IProjectRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateSafetyProjectCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateSafetyProjectCommandHandler(
        IProjectRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateSafetyProjectCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateSafetyProjectCommand request, CancellationToken cancellationToken)
    {
        var projectId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            projectId: projectId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            name: request.Name,
            description: request.Description,
            priority: request.Priority,
            projectOwnerId: request.ProjectOwnerId,
            projectManagerId: request.ProjectManagerId,
            plannedStartUtc: request.PlannedStartUtc,
            plannedEndUtc: request.PlannedEndUtc,
            plannedBudget: request.PlannedBudget,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "SafetyProject {ProjectId} created for Tenant {TenantId} by Actor {ActorId}",
            projectId, request.TenantId, request.ActorId);

        return projectId;
    }
}
