// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           GetJointResponsePlanByIncidentQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for GetJointResponsePlanByIncidentQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;

/// <summary>Handles <see cref="GetJointResponsePlanByIncidentQuery"/>.</summary>
public sealed class GetJointResponsePlanByIncidentQueryHandler : IRequestHandler<GetJointResponsePlanByIncidentQuery, Result<GetJointResponsePlanByIncidentResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<GetJointResponsePlanByIncidentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetJointResponsePlanByIncidentQueryHandler(ILocalEmergencyCoordinationRepository repository, ILogger<GetJointResponsePlanByIncidentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetJointResponsePlanByIncidentResult>> Handle(GetJointResponsePlanByIncidentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetJointResponsePlansByIncidentAsync(request.TenantId, request.IncidentId, cancellationToken);

            return Result<GetJointResponsePlanByIncidentResult>.Success(new GetJointResponsePlanByIncidentResult(
                request.TenantId, request.IncidentId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve joint response plans for incident {IncidentId}", request.IncidentId);

            return Result<GetJointResponsePlanByIncidentResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.GetJointResponsePlanByIncidentFailed",
                "Unable to retrieve joint response plans at this time.",
                Guid.NewGuid()));
        }
    }
}
