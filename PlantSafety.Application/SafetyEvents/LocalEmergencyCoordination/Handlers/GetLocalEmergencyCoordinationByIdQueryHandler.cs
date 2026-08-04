// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           GetLocalEmergencyCoordinationByIdQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for GetLocalEmergencyCoordinationByIdQuery. Mirrors GetEmergencyManagementByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;

/// <summary>Handles <see cref="GetLocalEmergencyCoordinationByIdQuery"/>.</summary>
public sealed class GetLocalEmergencyCoordinationByIdQueryHandler : IRequestHandler<GetLocalEmergencyCoordinationByIdQuery, Result<LocalEmergencyCoordinationItemDto>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<GetLocalEmergencyCoordinationByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLocalEmergencyCoordinationByIdQueryHandler(ILocalEmergencyCoordinationRepository repository, ILogger<GetLocalEmergencyCoordinationByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LocalEmergencyCoordinationItemDto>> Handle(GetLocalEmergencyCoordinationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.LocalEmergencyCoordinationId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("LocalEmergencyCoordination {LocalEmergencyCoordinationId} not found for Tenant {TenantId}", request.LocalEmergencyCoordinationId, request.TenantId);

                return Result<LocalEmergencyCoordinationItemDto>.Failure(new ApplicationError(
                    "SafetyEvents.LocalEmergencyCoordination.NotFound",
                    "The requested local emergency coordination case could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<LocalEmergencyCoordinationItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LocalEmergencyCoordination {LocalEmergencyCoordinationId}", request.LocalEmergencyCoordinationId);

            return Result<LocalEmergencyCoordinationItemDto>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.RetrievalFailed",
                "Unable to retrieve the local emergency coordination case at this time.",
                Guid.NewGuid()));
        }
    }
}
