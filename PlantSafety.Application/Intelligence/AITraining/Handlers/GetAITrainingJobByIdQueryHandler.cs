// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           GetAITrainingJobByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetAITrainingJobByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  Mirrors PlantSafety.Application.ContinuousImprovement.RCA.Handlers
//                  .GetRCAByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.AITraining.Models;
using PlantSafety.Application.Intelligence.AITraining.Queries;
using PlantSafety.Application.Intelligence.AITraining.Repositories;

namespace PlantSafety.Application.Intelligence.AITraining.Handlers;

/// <summary>Handles <see cref="GetAITrainingJobByIdQuery"/>.</summary>
public sealed class GetAITrainingJobByIdQueryHandler : IRequestHandler<GetAITrainingJobByIdQuery, Result<AITrainingJobDetailDto>>
{
    private readonly IAITrainingJobDetailRepository _repository;
    private readonly ILogger<GetAITrainingJobByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAITrainingJobByIdQueryHandler(IAITrainingJobDetailRepository repository, ILogger<GetAITrainingJobByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AITrainingJobDetailDto>> Handle(GetAITrainingJobByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.AITrainingJobId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("AITrainingJob {AITrainingJobId} not found for Tenant {TenantId}", request.AITrainingJobId, request.TenantId);

                return Result<AITrainingJobDetailDto>.Failure(new ApplicationError(
                    "Intelligence.AITraining.NotFound",
                    "The requested AI training job could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<AITrainingJobDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AITrainingJob {AITrainingJobId}", request.AITrainingJobId);

            return Result<AITrainingJobDetailDto>.Failure(new ApplicationError(
                "Intelligence.AITraining.RetrievalFailed",
                "Unable to retrieve AI training job data at this time.",
                Guid.NewGuid()));
        }
    }
}
