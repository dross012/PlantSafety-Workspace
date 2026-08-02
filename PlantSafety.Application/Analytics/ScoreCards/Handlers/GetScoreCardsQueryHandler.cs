// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           GetScoreCardsQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetScoreCardsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Repositories;

namespace PlantSafety.Application.Analytics.ScoreCards.Handlers;

/// <summary>Handles <see cref="GetScoreCardsQuery"/>.</summary>
public sealed class GetScoreCardsQueryHandler : IRequestHandler<GetScoreCardsQuery, Result<ScoreCardsQueryResult>>
{
    private readonly IScoreCardsRepository _repository;
    private readonly ILogger<GetScoreCardsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetScoreCardsQueryHandler(IScoreCardsRepository repository, ILogger<GetScoreCardsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScoreCardsQueryResult>> Handle(GetScoreCardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetScoreCardsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ScoreCardsQueryResult>.Success(new ScoreCardsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ScoreCards for Tenant {TenantId}", request.TenantId);

            return Result<ScoreCardsQueryResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.RetrievalFailed",
                "Unable to retrieve ScoreCards data at this time.",
                Guid.NewGuid()));
        }
    }
}
