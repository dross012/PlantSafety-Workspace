// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           GetPredictiveSafetyAlertByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPredictiveSafetyAlertByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction). Returns
//                  a NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors PlantSafety.Application.ContinuousImprovement.RCA.Handlers
//                  .GetRCAByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;
using PlantSafety.Application.Intelligence.PredictiveSafety.Queries;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Handlers;

/// <summary>Handles <see cref="GetPredictiveSafetyAlertByIdQuery"/>.</summary>
public sealed class GetPredictiveSafetyAlertByIdQueryHandler : IRequestHandler<GetPredictiveSafetyAlertByIdQuery, Result<PredictiveSafetyAlertDetailDto>>
{
    private readonly IPredictiveSafetyAlertDetailRepository _repository;
    private readonly ILogger<GetPredictiveSafetyAlertByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveSafetyAlertByIdQueryHandler(IPredictiveSafetyAlertDetailRepository repository, ILogger<GetPredictiveSafetyAlertByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveSafetyAlertDetailDto>> Handle(GetPredictiveSafetyAlertByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PredictiveSafetyAlertId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("PredictiveSafetyAlert {PredictiveSafetyAlertId} not found for Tenant {TenantId}", request.PredictiveSafetyAlertId, request.TenantId);

                return Result<PredictiveSafetyAlertDetailDto>.Failure(new ApplicationError(
                    "Intelligence.PredictiveSafety.NotFound",
                    "The requested predictive safety alert could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PredictiveSafetyAlertDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveSafetyAlert {PredictiveSafetyAlertId}", request.PredictiveSafetyAlertId);

            return Result<PredictiveSafetyAlertDetailDto>.Failure(new ApplicationError(
                "Intelligence.PredictiveSafety.RetrievalFailed",
                "Unable to retrieve predictive safety alert data at this time.",
                Guid.NewGuid()));
        }
    }
}
