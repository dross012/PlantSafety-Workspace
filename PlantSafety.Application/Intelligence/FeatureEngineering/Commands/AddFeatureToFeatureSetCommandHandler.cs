// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           AddFeatureToFeatureSetCommandHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for AddFeatureToFeatureSetCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. A
//                  FeatureSetAlreadyPublishedException raised by the domain is mapped to a Domain
//                  ApplicationError.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Commands;

/// <summary>Handles <see cref="AddFeatureToFeatureSetCommand"/>.</summary>
public sealed class AddFeatureToFeatureSetCommandHandler : IRequestHandler<AddFeatureToFeatureSetCommand, Result<FeatureSetStatusResult>>
{
    private readonly IFeatureSetRepository _repository;
    private readonly ILogger<AddFeatureToFeatureSetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddFeatureToFeatureSetCommandHandler(IFeatureSetRepository repository, ILogger<AddFeatureToFeatureSetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureSetStatusResult>> Handle(AddFeatureToFeatureSetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = FeatureSetId.FromGuid(request.FeatureSetId);
            var featureSet = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (featureSet is null)
            {
                return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                    "Intelligence.FeatureEngineering.NotFound",
                    "The requested feature set could not be found.",
                    request.FeatureSetId,
                    ErrorSeverity.NotFound));
            }

            featureSet.AddFeature(
                FeatureName.From(request.FeatureName),
                new PlantSafety.SharedKernel.Identifiers.CorrelationId(request.CorrelationId),
                new PlantSafety.SharedKernel.Identifiers.CausationId(request.CausationId));

            await _repository.UpdateAsync(featureSet, request.ETag, cancellationToken);

            return Result<FeatureSetStatusResult>.Success(new FeatureSetStatusResult(
                featureSet.FeatureSetId.Value,
                featureSet.IsPublished,
                featureSet.Features.Count));
        }
        catch (FeatureSetAlreadyPublishedException ex)
        {
            return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.AlreadyPublished",
                ex.Message,
                Guid.NewGuid(),
                ErrorSeverity.Domain));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add feature to FeatureSet {FeatureSetId}", request.FeatureSetId);

            return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.AddFeatureFailed",
                "Unable to add the feature at this time.",
                Guid.NewGuid()));
        }
    }
}
