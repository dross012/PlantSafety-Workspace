// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           GetOptimizationRunHistoryQueryHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for GetOptimizationRunHistoryQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Queries;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Repositories;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Handlers;

/// <summary>Handles <see cref="GetOptimizationRunHistoryQuery"/>.</summary>
public sealed class GetOptimizationRunHistoryQueryHandler : IRequestHandler<GetOptimizationRunHistoryQuery, Result<IReadOnlyList<OptimizationRunSummaryDto>>>
{
    private readonly IOptimizationRunReadRepository _repository;
    private readonly ILogger<GetOptimizationRunHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOptimizationRunHistoryQueryHandler(IOptimizationRunReadRepository repository, ILogger<GetOptimizationRunHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<OptimizationRunSummaryDto>>> Handle(GetOptimizationRunHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;
            var fromUtc = request.FromUtc ?? toUtc.AddDays(-30);

            var items = await _repository.ListHistoryAsync(request.TenantId, request.SiteId, fromUtc, toUtc, request.StatusCode, cancellationToken);
            return Result<IReadOnlyList<OptimizationRunSummaryDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve OptimizationRun history for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<OptimizationRunSummaryDto>>.Failure(new ApplicationError(
                "PredictiveScheduling.OptimizationRun.HistoryRetrievalFailed",
                "Unable to retrieve optimization run history at this time.",
                Guid.NewGuid()));
        }
    }
}
