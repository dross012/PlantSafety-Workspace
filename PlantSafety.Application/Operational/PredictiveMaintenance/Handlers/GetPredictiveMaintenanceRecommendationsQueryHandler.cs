// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceRecommendationsQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceRecommendationsQuery. Builds the single
//                  canonical recommendation for an alert (RecommendationId == AlertId; see
//                  PredictiveMaintenanceRecommendationDto remarks). Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceRecommendationsQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceRecommendationsQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceRecommendationsQuery, Result<PaginatedResult<PredictiveMaintenanceRecommendationDto>>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<GetPredictiveMaintenanceRecommendationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceRecommendationsQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceRecommendationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PredictiveMaintenanceRecommendationDto>>> Handle(GetPredictiveMaintenanceRecommendationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _repository.GetByIdAsync(request.PredictionId, request.TenantId, cancellationToken);
            if (alert is null)
            {
                return Result<PaginatedResult<PredictiveMaintenanceRecommendationDto>>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.Recommendations.NotFound",
                    "The requested prediction could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var recommendation = PredictiveMaintenanceRecommendationFactory.Build(alert);
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);

            return Result<PaginatedResult<PredictiveMaintenanceRecommendationDto>>.Success(
                new PaginatedResult<PredictiveMaintenanceRecommendationDto>([recommendation], 1, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build recommendations for PredictiveMaintenanceAlert {PredictionId}", request.PredictionId);

            return Result<PaginatedResult<PredictiveMaintenanceRecommendationDto>>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.Recommendations.RetrievalFailed",
                "Unable to retrieve recommendations at this time.",
                Guid.NewGuid()));
        }
    }
}
