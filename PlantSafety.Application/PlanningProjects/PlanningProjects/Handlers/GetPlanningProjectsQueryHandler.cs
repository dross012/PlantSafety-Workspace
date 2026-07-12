// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPlanningProjectsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PlanningProjects.PlanningProjects.Queries;
using PlantSafety.Application.PlanningProjects.PlanningProjects.Repositories;

namespace PlantSafety.Application.PlanningProjects.PlanningProjects.Handlers;

/// <summary>Handles <see cref="GetPlanningProjectsQuery"/>.</summary>
public sealed class GetPlanningProjectsQueryHandler : IRequestHandler<GetPlanningProjectsQuery, Result<PlanningProjectsQueryResult>>
{
    private readonly IPlanningProjectsRepository _repository;
    private readonly ILogger<GetPlanningProjectsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPlanningProjectsQueryHandler(IPlanningProjectsRepository repository, ILogger<GetPlanningProjectsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PlanningProjectsQueryResult>> Handle(GetPlanningProjectsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetProjectsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PlanningProjectsQueryResult>.Success(new PlanningProjectsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PlanningProjects for Tenant {TenantId}", request.TenantId);

            return Result<PlanningProjectsQueryResult>.Failure(new ApplicationError(
                "PlanningProjects.PlanningProjects.RetrievalFailed",
                "Unable to retrieve planning project data at this time.",
                Guid.NewGuid()));
        }
    }
}
