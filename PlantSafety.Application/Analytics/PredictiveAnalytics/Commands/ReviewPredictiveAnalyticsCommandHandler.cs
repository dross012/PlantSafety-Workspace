// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           ReviewPredictiveAnalyticsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ReviewPredictiveAnalyticsCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;

/// <summary>Handles <see cref="ReviewPredictiveAnalyticsCommand"/>.</summary>
public sealed class ReviewPredictiveAnalyticsCommandHandler : IRequestHandler<ReviewPredictiveAnalyticsCommand, Result<PredictiveAnalyticsStatusResult>>
{
    private readonly IPredictiveAnalyticsRepository _repository;
    private readonly ILogger<ReviewPredictiveAnalyticsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReviewPredictiveAnalyticsCommandHandler(IPredictiveAnalyticsRepository repository, ILogger<ReviewPredictiveAnalyticsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveAnalyticsStatusResult>> Handle(ReviewPredictiveAnalyticsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PredictiveAnalyticsId.FromGuid(request.PredictiveAnalyticsId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PredictiveAnalyticsStatusResult>.Failure(new ApplicationError(
                    "Analytics.PredictiveAnalytics.NotFound",
                    "The requested predictive analytics record could not be found.",
                    request.PredictiveAnalyticsId,
                    ErrorSeverity.NotFound));
            }

            var reviewedBy = UserId.FromGuid(request.ReviewedByUserId);

            aggregate.Review(
                reviewedBy: reviewedBy,
                reviewedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(reviewedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PredictiveAnalyticsStatusResult>.Success(new PredictiveAnalyticsStatusResult(
                aggregate.PredictiveAnalyticsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to review PredictiveAnalytics {PredictiveAnalyticsId}", request.PredictiveAnalyticsId);

            return Result<PredictiveAnalyticsStatusResult>.Failure(new ApplicationError(
                "Analytics.PredictiveAnalytics.ReviewFailed",
                "Unable to review the predictive analytics record at this time.",
                Guid.NewGuid()));
        }
    }
}
