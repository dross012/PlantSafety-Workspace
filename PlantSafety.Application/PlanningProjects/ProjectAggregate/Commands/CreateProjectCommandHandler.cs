// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           CreateProjectCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateProjectCommand. Pure orchestration: constructs the Project
//                  aggregate via its domain factory method and persists it through the domain
//                  repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PlanningProjects.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

/// <summary>Handles <see cref="CreateProjectCommand"/>.</summary>
public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectStatusResult>>
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<CreateProjectCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateProjectCommandHandler(IProjectRepository repository, ILogger<CreateProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProjectStatusResult>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var owner = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.PlanningProjects.Aggregates.Project.Create(
                tenantId: request.TenantId,
                siteId: request.SiteId,
                name: request.Name,
                targetCompletionUtc: UtcTimestamp.From(request.TargetCompletionUtc),
                ownerId: owner,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(owner));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<ProjectStatusResult>.Success(new ProjectStatusResult(
                aggregate.ProjectId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Project for Tenant {TenantId}", request.TenantId);

            return Result<ProjectStatusResult>.Failure(new ApplicationError(
                "PlanningProjects.Project.CreateFailed",
                "Unable to create the project at this time.",
                Guid.NewGuid()));
        }
    }
}
