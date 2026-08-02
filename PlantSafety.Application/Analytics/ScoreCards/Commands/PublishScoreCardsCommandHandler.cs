// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           PublishScoreCardsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for PublishScoreCardsCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
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

/// <summary>Handles <see cref="PublishScoreCardsCommand"/>.</summary>
public sealed class PublishScoreCardsCommandHandler : IRequestHandler<PublishScoreCardsCommand, Result<ScoreCardsStatusResult>>
{
    private readonly IScoreCardsRepository _repository;
    private readonly ILogger<PublishScoreCardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishScoreCardsCommandHandler(IScoreCardsRepository repository, ILogger<PublishScoreCardsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScoreCardsStatusResult>> Handle(PublishScoreCardsCommand request, CancellationToken cancellationToken)
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

            var publishedBy = UserId.FromGuid(request.PublishedByUserId);

            aggregate.Publish(
                publishedBy: publishedBy,
                publishedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(publishedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ScoreCardsStatusResult>.Success(new ScoreCardsStatusResult(
                aggregate.ScoreCardsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409 --
            // swallowing it here as a generic ApplicationError would misreport a stale-ETag
            // conflict as an ordinary 400 failure.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ScoreCards {ScoreCardsId}", request.ScoreCardsId);

            return Result<ScoreCardsStatusResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.PublishFailed",
                "Unable to publish the scorecard at this time.",
                Guid.NewGuid()));
        }
    }
}
