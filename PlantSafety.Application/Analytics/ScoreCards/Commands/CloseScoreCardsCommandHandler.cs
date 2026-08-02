// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           CloseScoreCardsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseScoreCardsCommand.
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

/// <summary>Handles <see cref="CloseScoreCardsCommand"/>.</summary>
public sealed class CloseScoreCardsCommandHandler : IRequestHandler<CloseScoreCardsCommand, Result<ScoreCardsStatusResult>>
{
    private readonly IScoreCardsRepository _repository;
    private readonly ILogger<CloseScoreCardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseScoreCardsCommandHandler(IScoreCardsRepository repository, ILogger<CloseScoreCardsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScoreCardsStatusResult>> Handle(CloseScoreCardsCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Close(
                closedBy: performedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
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
            _logger.LogError(ex, "Failed to close ScoreCards {ScoreCardsId}", request.ScoreCardsId);

            return Result<ScoreCardsStatusResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.CloseFailed",
                "Unable to close the scorecard at this time.",
                Guid.NewGuid()));
        }
    }
}
