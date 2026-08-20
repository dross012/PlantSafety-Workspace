// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           GetFeatureSetByIdQueryHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for GetFeatureSetByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-entity reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  Mirrors PlantSafety.Application.Intelligence.DataLake.Handlers
//                  .GetDataLakeActivityByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.FeatureEngineering.Models;
using PlantSafety.Application.Intelligence.FeatureEngineering.Queries;
using PlantSafety.Application.Intelligence.FeatureEngineering.Repositories;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Handlers;

/// <summary>Handles <see cref="GetFeatureSetByIdQuery"/>.</summary>
public sealed class GetFeatureSetByIdQueryHandler : IRequestHandler<GetFeatureSetByIdQuery, Result<FeatureSetDetailDto>>
{
    private readonly IFeatureSetDetailRepository _repository;
    private readonly ILogger<GetFeatureSetByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFeatureSetByIdQueryHandler(IFeatureSetDetailRepository repository, ILogger<GetFeatureSetByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureSetDetailDto>> Handle(GetFeatureSetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.FeatureSetId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("FeatureSet {FeatureSetId} not found for Tenant {TenantId}", request.FeatureSetId, request.TenantId);

                return Result<FeatureSetDetailDto>.Failure(new ApplicationError(
                    "Intelligence.FeatureEngineering.NotFound",
                    "The requested feature set could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<FeatureSetDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve FeatureSet {FeatureSetId}", request.FeatureSetId);

            return Result<FeatureSetDetailDto>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.RetrievalFailed",
                "Unable to retrieve feature set data at this time.",
                Guid.NewGuid()));
        }
    }
}
