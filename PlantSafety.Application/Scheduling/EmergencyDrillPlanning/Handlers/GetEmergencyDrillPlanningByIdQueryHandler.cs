// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           GetEmergencyDrillPlanningByIdQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetEmergencyDrillPlanningByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction). Returns
//                  a NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. tenantId is pushed into the repository call itself
//                  (usp_EmergencyDrillPlanning_GetById's WHERE clause), not just checked here after
//                  the fact -- the post-fetch TenantId comparison below is defense-in-depth, not the
//                  primary control. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Queries;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Handlers;

/// <summary>Handles <see cref="GetEmergencyDrillPlanningByIdQuery"/>.</summary>
public sealed class GetEmergencyDrillPlanningByIdQueryHandler : IRequestHandler<GetEmergencyDrillPlanningByIdQuery, Result<EmergencyDrillPlanningItemDto>>
{
    private readonly IEmergencyDrillPlanningRepository _repository;
    private readonly ILogger<GetEmergencyDrillPlanningByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyDrillPlanningByIdQueryHandler(IEmergencyDrillPlanningRepository repository, ILogger<GetEmergencyDrillPlanningByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyDrillPlanningItemDto>> Handle(GetEmergencyDrillPlanningByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.EmergencyDrillPlanningId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("EmergencyDrillPlanning {EmergencyDrillPlanningId} not found for Tenant {TenantId}", request.EmergencyDrillPlanningId, request.TenantId);

                return Result<EmergencyDrillPlanningItemDto>.Failure(new ApplicationError(
                    "Scheduling.EmergencyDrillPlanning.NotFound",
                    "The requested emergency drill could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<EmergencyDrillPlanningItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyDrillPlanning {EmergencyDrillPlanningId}", request.EmergencyDrillPlanningId);

            return Result<EmergencyDrillPlanningItemDto>.Failure(new ApplicationError(
                "Scheduling.EmergencyDrillPlanning.RetrievalFailed",
                "Unable to retrieve emergency drill data at this time.",
                Guid.NewGuid()));
        }
    }
}
