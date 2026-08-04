// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GetTaskComplexityProfileQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetTaskComplexityProfileQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;

/// <summary>Handles <see cref="GetTaskComplexityProfileQuery"/>.</summary>
public sealed class GetTaskComplexityProfileQueryHandler : IRequestHandler<GetTaskComplexityProfileQuery, Result<TaskComplexityProfileDto>>
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repository;
    private readonly ILogger<GetTaskComplexityProfileQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTaskComplexityProfileQueryHandler(ICognitiveLoadAnalyticsReadRepository repository, ILogger<GetTaskComplexityProfileQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TaskComplexityProfileDto>> Handle(GetTaskComplexityProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetTaskComplexityProfileAsync(request.TenantId, request.TaskCode, cancellationToken);

            if (item is null)
            {
                return Result<TaskComplexityProfileDto>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.TaskComplexityProfile.NotFound",
                    $"No task complexity profile was found for task code '{request.TaskCode}'.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<TaskComplexityProfileDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve task complexity profile for Tenant {TenantId}", request.TenantId);

            return Result<TaskComplexityProfileDto>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.TaskComplexityProfile.RetrievalFailed",
                "Unable to retrieve the task complexity profile at this time.",
                Guid.NewGuid()));
        }
    }
}
