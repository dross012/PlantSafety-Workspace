// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           ClosePredictiveAnalyticsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ClosePredictiveAnalyticsCommand.
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

/// <summary>Handles <see cref="ClosePredictiveAnalyticsCommand"/>.</summary>
public sealed class ClosePredictiveAnalyticsCommandHandler : IRequestHandler<ClosePredictiveAnalyticsCommand, Result<PredictiveAnalyticsStatusResult>>
{
    private readonly IPredictiveAnalyticsRepository _repository;
    private readonly ILogger<ClosePredictiveAnalyticsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ClosePredictiveAnalyticsCommandHandler(IPredictiveAnalyticsRepository repository, ILogger<ClosePredictiveAnalyticsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveAnalyticsStatusResult>> Handle(ClosePredictiveAnalyticsCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Close(
                closedBy: performedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
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
            _logger.LogError(ex, "Failed to close PredictiveAnalytics {PredictiveAnalyticsId}", request.PredictiveAnalyticsId);

            return Result<PredictiveAnalyticsStatusResult>.Failure(new ApplicationError(
                "Analytics.PredictiveAnalytics.CloseFailed",
                "Unable to close the predictive analytics record at this time.",
                Guid.NewGuid()));
        }
    }
}
