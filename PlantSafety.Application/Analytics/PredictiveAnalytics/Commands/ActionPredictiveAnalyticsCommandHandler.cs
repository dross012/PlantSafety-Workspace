// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           ActionPredictiveAnalyticsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActionPredictiveAnalyticsCommand.
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

/// <summary>Handles <see cref="ActionPredictiveAnalyticsCommand"/>.</summary>
public sealed class ActionPredictiveAnalyticsCommandHandler : IRequestHandler<ActionPredictiveAnalyticsCommand, Result<PredictiveAnalyticsStatusResult>>
{
    private readonly IPredictiveAnalyticsRepository _repository;
    private readonly ILogger<ActionPredictiveAnalyticsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActionPredictiveAnalyticsCommandHandler(IPredictiveAnalyticsRepository repository, ILogger<ActionPredictiveAnalyticsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveAnalyticsStatusResult>> Handle(ActionPredictiveAnalyticsCommand request, CancellationToken cancellationToken)
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

            var actionedBy = UserId.FromGuid(request.ActionedByUserId);

            aggregate.Action(
                actionedBy: actionedBy,
                actionNotes: request.ActionNotes,
                actionedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actionedBy),
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
            _logger.LogError(ex, "Failed to action PredictiveAnalytics {PredictiveAnalyticsId}", request.PredictiveAnalyticsId);

            return Result<PredictiveAnalyticsStatusResult>.Failure(new ApplicationError(
                "Analytics.PredictiveAnalytics.ActionFailed",
                "Unable to action the predictive analytics record at this time.",
                Guid.NewGuid()));
        }
    }
}
