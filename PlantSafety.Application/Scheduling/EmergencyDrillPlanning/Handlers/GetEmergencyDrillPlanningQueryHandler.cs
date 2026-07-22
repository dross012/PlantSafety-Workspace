// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEmergencyDrillPlanningQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Handlers;

/// <summary>Handles <see cref="GetEmergencyDrillPlanningQuery"/>.</summary>
public sealed class GetEmergencyDrillPlanningQueryHandler : IRequestHandler<GetEmergencyDrillPlanningQuery, Result<EmergencyDrillPlanningQueryResult>>
{
    private readonly IEmergencyDrillPlanningRepository _repository;
    private readonly ILogger<GetEmergencyDrillPlanningQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyDrillPlanningQueryHandler(IEmergencyDrillPlanningRepository repository, ILogger<GetEmergencyDrillPlanningQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyDrillPlanningQueryResult>> Handle(GetEmergencyDrillPlanningQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergencyDrillPlanningAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyDrillPlanningQueryResult>.Success(new EmergencyDrillPlanningQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyDrillPlanning for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyDrillPlanningQueryResult>.Failure(new ApplicationError(
                "Scheduling.EmergencyDrillPlanning.RetrievalFailed",
                "Unable to retrieve EmergencyDrillPlanning data at this time.",
                Guid.NewGuid()));
        }
    }
}