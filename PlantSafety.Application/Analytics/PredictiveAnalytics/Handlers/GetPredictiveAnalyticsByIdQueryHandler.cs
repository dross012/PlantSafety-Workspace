// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           GetPredictiveAnalyticsByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPredictiveAnalyticsByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction).
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Models;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Queries;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Repositories;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Handlers;

/// <summary>Handles <see cref="GetPredictiveAnalyticsByIdQuery"/>.</summary>
public sealed class GetPredictiveAnalyticsByIdQueryHandler : IRequestHandler<GetPredictiveAnalyticsByIdQuery, Result<PredictiveAnalyticsItemDto>>
{
    private readonly IPredictiveAnalyticsRepository _repository;
    private readonly ILogger<GetPredictiveAnalyticsByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveAnalyticsByIdQueryHandler(IPredictiveAnalyticsRepository repository, ILogger<GetPredictiveAnalyticsByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveAnalyticsItemDto>> Handle(GetPredictiveAnalyticsByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PredictiveAnalyticsId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("PredictiveAnalytics {PredictiveAnalyticsId} not found for Tenant {TenantId}", request.PredictiveAnalyticsId, request.TenantId);

                return Result<PredictiveAnalyticsItemDto>.Failure(new ApplicationError(
                    "Analytics.PredictiveAnalytics.NotFound",
                    "The requested predictive analytics record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PredictiveAnalyticsItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveAnalytics {PredictiveAnalyticsId}", request.PredictiveAnalyticsId);

            return Result<PredictiveAnalyticsItemDto>.Failure(new ApplicationError(
                "Analytics.PredictiveAnalytics.RetrievalFailed",
                "Unable to retrieve predictive analytics data at this time.",
                Guid.NewGuid()));
        }
    }
}
