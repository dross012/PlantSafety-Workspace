// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           GetOptimizationRunByIdQueryHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for GetOptimizationRunByIdQuery.
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

/// <summary>Handles <see cref="GetOptimizationRunByIdQuery"/>.</summary>
public sealed class GetOptimizationRunByIdQueryHandler : IRequestHandler<GetOptimizationRunByIdQuery, Result<OptimizationRunDto>>
{
    private readonly IOptimizationRunReadRepository _repository;
    private readonly ILogger<GetOptimizationRunByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOptimizationRunByIdQueryHandler(IOptimizationRunReadRepository repository, ILogger<GetOptimizationRunByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OptimizationRunDto>> Handle(GetOptimizationRunByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.OptimizationRunId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("OptimizationRun {OptimizationRunId} not found for Tenant {TenantId}", request.OptimizationRunId, request.TenantId);

                return Result<OptimizationRunDto>.Failure(new ApplicationError(
                    "PredictiveScheduling.OptimizationRun.NotFound",
                    "The requested optimization run could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<OptimizationRunDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve OptimizationRun {OptimizationRunId}", request.OptimizationRunId);

            return Result<OptimizationRunDto>.Failure(new ApplicationError(
                "PredictiveScheduling.OptimizationRun.RetrievalFailed",
                "Unable to retrieve optimization run data at this time.",
                Guid.NewGuid()));
        }
    }
}
