// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           ListElevatedCognitiveRiskPredictionsQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles ListElevatedCognitiveRiskPredictionsQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;

/// <summary>Handles <see cref="ListElevatedCognitiveRiskPredictionsQuery"/>.</summary>
public sealed class ListElevatedCognitiveRiskPredictionsQueryHandler : IRequestHandler<ListElevatedCognitiveRiskPredictionsQuery, Result<IReadOnlyList<CognitiveRiskPredictionDto>>>
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repository;
    private readonly ILogger<ListElevatedCognitiveRiskPredictionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListElevatedCognitiveRiskPredictionsQueryHandler(ICognitiveLoadAnalyticsReadRepository repository, ILogger<ListElevatedCognitiveRiskPredictionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<CognitiveRiskPredictionDto>>> Handle(ListElevatedCognitiveRiskPredictionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListElevatedCognitiveRiskPredictionsAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<IReadOnlyList<CognitiveRiskPredictionDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list elevated cognitive risk predictions for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<CognitiveRiskPredictionDto>>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.ListElevatedCognitiveRiskPredictions.RetrievalFailed",
                "Unable to retrieve elevated cognitive risk predictions at this time.",
                Guid.NewGuid()));
        }
    }
}
