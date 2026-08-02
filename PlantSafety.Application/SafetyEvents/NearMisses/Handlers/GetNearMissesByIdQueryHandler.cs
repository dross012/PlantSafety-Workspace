// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           GetNearMissesByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetNearMissesByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Handlers
//                  .GetPredictiveMaintenanceAlertByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.NearMisses.Models;
using PlantSafety.Application.SafetyEvents.NearMisses.Queries;
using PlantSafety.Application.SafetyEvents.NearMisses.Repositories;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Handlers;

/// <summary>Handles <see cref="GetNearMissesByIdQuery"/>.</summary>
public sealed class GetNearMissesByIdQueryHandler : IRequestHandler<GetNearMissesByIdQuery, Result<NearMissesDetailDto>>
{
    private readonly INearMissesRepository _repository;
    private readonly ILogger<GetNearMissesByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetNearMissesByIdQueryHandler(INearMissesRepository repository, ILogger<GetNearMissesByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissesDetailDto>> Handle(GetNearMissesByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.NearMissesId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("NearMisses {NearMissesId} not found for Tenant {TenantId}", request.NearMissesId, request.TenantId);

                return Result<NearMissesDetailDto>.Failure(new ApplicationError(
                    "SafetyEvents.NearMisses.NotFound",
                    "The requested near miss could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<NearMissesDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve NearMisses {NearMissesId}", request.NearMissesId);

            return Result<NearMissesDetailDto>.Failure(new ApplicationError(
                "SafetyEvents.NearMisses.RetrievalFailed",
                "Unable to retrieve near miss data at this time.",
                Guid.NewGuid()));
        }
    }
}
