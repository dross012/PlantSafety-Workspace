// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GetCognitiveLoadByWorkerQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetCognitiveLoadByWorkerQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;

/// <summary>Handles <see cref="GetCognitiveLoadByWorkerQuery"/>.</summary>
public sealed class GetCognitiveLoadByWorkerQueryHandler : IRequestHandler<GetCognitiveLoadByWorkerQuery, Result<CognitiveLoadByWorkerDto>>
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repository;
    private readonly ILogger<GetCognitiveLoadByWorkerQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCognitiveLoadByWorkerQueryHandler(ICognitiveLoadAnalyticsReadRepository repository, ILogger<GetCognitiveLoadByWorkerQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CognitiveLoadByWorkerDto>> Handle(GetCognitiveLoadByWorkerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetCognitiveLoadByWorkerAsync(
                request.TenantId, request.WorkerUserId, request.PeriodStartUtc, request.PeriodEndUtc, cancellationToken);

            if (item is null)
            {
                return Result<CognitiveLoadByWorkerDto>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.CognitiveLoadByWorker.NotFound",
                    "No cognitive load readings were found for the requested worker/period.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<CognitiveLoadByWorkerDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cognitive load by worker for Tenant {TenantId}", request.TenantId);

            return Result<CognitiveLoadByWorkerDto>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.CognitiveLoadByWorker.RetrievalFailed",
                "Unable to retrieve cognitive load data at this time.",
                Guid.NewGuid()));
        }
    }
}
