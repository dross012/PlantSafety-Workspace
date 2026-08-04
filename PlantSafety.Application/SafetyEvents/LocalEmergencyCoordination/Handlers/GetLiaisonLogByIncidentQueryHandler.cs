// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           GetLiaisonLogByIncidentQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for GetLiaisonLogByIncidentQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;

/// <summary>Handles <see cref="GetLiaisonLogByIncidentQuery"/>.</summary>
public sealed class GetLiaisonLogByIncidentQueryHandler : IRequestHandler<GetLiaisonLogByIncidentQuery, Result<GetLiaisonLogByIncidentResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<GetLiaisonLogByIncidentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLiaisonLogByIncidentQueryHandler(ILocalEmergencyCoordinationRepository repository, ILogger<GetLiaisonLogByIncidentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetLiaisonLogByIncidentResult>> Handle(GetLiaisonLogByIncidentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLiaisonLogByIncidentAsync(request.TenantId, request.IncidentId, cancellationToken);

            return Result<GetLiaisonLogByIncidentResult>.Success(new GetLiaisonLogByIncidentResult(
                request.TenantId, request.IncidentId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve liaison log for incident {IncidentId}", request.IncidentId);

            return Result<GetLiaisonLogByIncidentResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.GetLiaisonLogByIncidentFailed",
                "Unable to retrieve the liaison communication log at this time.",
                Guid.NewGuid()));
        }
    }
}
