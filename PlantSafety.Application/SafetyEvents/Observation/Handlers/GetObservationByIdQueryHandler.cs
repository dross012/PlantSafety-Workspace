// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           GetObservationByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetObservationByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.Observation.Models;
using PlantSafety.Application.SafetyEvents.Observation.Queries;
using PlantSafety.Application.SafetyEvents.Observation.Repositories;

namespace PlantSafety.Application.SafetyEvents.Observation.Handlers;

/// <summary>Handles <see cref="GetObservationByIdQuery"/>.</summary>
public sealed class GetObservationByIdQueryHandler : IRequestHandler<GetObservationByIdQuery, Result<ObservationItemDto>>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<GetObservationByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetObservationByIdQueryHandler(IObservationRepository repository, ILogger<GetObservationByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ObservationItemDto>> Handle(GetObservationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ObservationId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Observation {ObservationId} not found for Tenant {TenantId}", request.ObservationId, request.TenantId);

                return Result<ObservationItemDto>.Failure(new ApplicationError(
                    "SafetyEvents.Observation.NotFound",
                    "The requested observation could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ObservationItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Observation {ObservationId}", request.ObservationId);

            return Result<ObservationItemDto>.Failure(new ApplicationError(
                "SafetyEvents.Observation.RetrievalFailed",
                "Unable to retrieve observation data at this time.",
                Guid.NewGuid()));
        }
    }
}
