// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           GetScoreCardsByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetScoreCardsByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Models;
using PlantSafety.Application.Analytics.ScoreCards.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Repositories;

namespace PlantSafety.Application.Analytics.ScoreCards.Handlers;

/// <summary>Handles <see cref="GetScoreCardsByIdQuery"/>.</summary>
public sealed class GetScoreCardsByIdQueryHandler : IRequestHandler<GetScoreCardsByIdQuery, Result<ScoreCardsItemDto>>
{
    private readonly IScoreCardsRepository _repository;
    private readonly ILogger<GetScoreCardsByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetScoreCardsByIdQueryHandler(IScoreCardsRepository repository, ILogger<GetScoreCardsByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScoreCardsItemDto>> Handle(GetScoreCardsByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ScoreCardsId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("ScoreCards {ScoreCardsId} not found for Tenant {TenantId}", request.ScoreCardsId, request.TenantId);

                return Result<ScoreCardsItemDto>.Failure(new ApplicationError(
                    "Analytics.ScoreCards.NotFound",
                    "The requested scorecard could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ScoreCardsItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ScoreCards {ScoreCardsId}", request.ScoreCardsId);

            return Result<ScoreCardsItemDto>.Failure(new ApplicationError(
                "Analytics.ScoreCards.RetrievalFailed",
                "Unable to retrieve scorecard data at this time.",
                Guid.NewGuid()));
        }
    }
}
