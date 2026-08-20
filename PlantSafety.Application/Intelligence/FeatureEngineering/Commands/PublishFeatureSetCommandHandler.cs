// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           PublishFeatureSetCommandHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Handler for PublishFeatureSetCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Domain
//                  publish-guard exceptions (FeatureSetAlreadyPublishedException,
//                  FeatureSetEmptyException) are mapped to Domain ApplicationErrors.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Commands;

/// <summary>Handles <see cref="PublishFeatureSetCommand"/>.</summary>
public sealed class PublishFeatureSetCommandHandler : IRequestHandler<PublishFeatureSetCommand, Result<FeatureSetStatusResult>>
{
    private readonly IFeatureSetRepository _repository;
    private readonly ILogger<PublishFeatureSetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishFeatureSetCommandHandler(IFeatureSetRepository repository, ILogger<PublishFeatureSetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureSetStatusResult>> Handle(PublishFeatureSetCommand request, CancellationToken cancellationToken)
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

            var publishedBy = UserId.FromGuid(request.PublishedByUserId);

            featureSet.Publish(
                publishedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(publishedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

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
        catch (FeatureSetEmptyException ex)
        {
            return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.EmptyFeatureSet",
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
            _logger.LogError(ex, "Failed to publish FeatureSet {FeatureSetId}", request.FeatureSetId);

            return Result<FeatureSetStatusResult>.Failure(new ApplicationError(
                "Intelligence.FeatureEngineering.PublishFailed",
                "Unable to publish the feature set at this time.",
                Guid.NewGuid()));
        }
    }
}
