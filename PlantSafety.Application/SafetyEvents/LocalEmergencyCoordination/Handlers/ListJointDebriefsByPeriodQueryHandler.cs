// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ListJointDebriefsByPeriodQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for ListJointDebriefsByPeriodQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;

/// <summary>Handles <see cref="ListJointDebriefsByPeriodQuery"/>.</summary>
public sealed class ListJointDebriefsByPeriodQueryHandler : IRequestHandler<ListJointDebriefsByPeriodQuery, Result<ListJointDebriefsByPeriodResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<ListJointDebriefsByPeriodQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListJointDebriefsByPeriodQueryHandler(ILocalEmergencyCoordinationRepository repository, ILogger<ListJointDebriefsByPeriodQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListJointDebriefsByPeriodResult>> Handle(ListJointDebriefsByPeriodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListJointDebriefsByPeriodAsync(request.TenantId, request.From, request.To, cancellationToken);

            return Result<ListJointDebriefsByPeriodResult>.Success(new ListJointDebriefsByPeriodResult(
                request.TenantId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list joint debriefs for Tenant {TenantId}", request.TenantId);

            return Result<ListJointDebriefsByPeriodResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.ListJointDebriefsByPeriodFailed",
                "Unable to retrieve joint debrief records at this time.",
                Guid.NewGuid()));
        }
    }
}
