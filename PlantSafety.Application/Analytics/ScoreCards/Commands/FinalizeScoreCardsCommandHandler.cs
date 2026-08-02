// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           FinalizeScoreCardsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for FinalizeScoreCardsCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.ScoreCards.Interfaces;
using PlantSafety.Domain.Analytics.ScoreCards.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.ScoreCards.Commands;

/// <summary>Handles <see cref="FinalizeScoreCardsCommand"/>.</summary>
public sealed class FinalizeScoreCardsCommandHandler : IRequestHandler<FinalizeScoreCardsCommand, Result<ScoreCardsStatusResult>>
{
    private readonly IScoreCardsRepository _repository;
    private readonly ILogger<FinalizeScoreCardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FinalizeScoreCardsCommandHandler(IScoreCardsRepository repository, ILogger<FinalizeScoreCardsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScoreCardsStatusResult>> Handle(FinalizeScoreCardsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ScoreCardsId.FromGuid(request.ScoreCardsId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ScoreCardsStatusResult>.Failure(new ApplicationError(
                    "Analytics.ScoreCards.NotFound",
                    "The requested scorecard could not be found.",
                    request.ScoreCardsId,
                    ErrorSeverity.NotFound));
            }

            var finalizedBy = UserId.FromGuid(request.FinalizedByUserId);

            aggregate.Finalize(
                finalizedBy: finalizedBy,
                finalizedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(finalizedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ScoreCardsStatusResult>.Success(new ScoreCardsStatusResult(
                aggregate.ScoreCardsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finalize ScoreCards {ScoreCardsId}", request.ScoreCardsId);

            return Result<ScoreCardsStatusResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.FinalizeFailed",
                "Unable to finalize the scorecard at this time.",
                Guid.NewGuid()));
        }
    }
}
