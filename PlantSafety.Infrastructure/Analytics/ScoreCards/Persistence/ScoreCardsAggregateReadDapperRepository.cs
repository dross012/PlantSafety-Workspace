// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Infrastructure
//  File:           ScoreCardsAggregateReadDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ScoreCards Application-layer read-model. Backs
//                  GetScoreCardsByIdQuery and GetScoreCardsQuery. Reuses
//                  usp_ScoreCardsAggregate_GetById (the same proc
//                  ScoreCardsAggregateDapperRepository.GetByIdAsync uses to hydrate the write-side
//                  aggregate) and usp_ScoreCardsAggregate_GetRange rather than introducing
//                  duplicate stored procedures. Named distinctly from the write-side
//                  ScoreCardsAggregateDapperRepository (and from the pre-existing, unregistered
//                  ScoreCardsRepository class already declared in this namespace) to avoid any
//                  simple-name ambiguity. Mirrors PermitsReadDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.ScoreCards.Models;
using PlantSafety.Application.Analytics.ScoreCards.Repositories;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Persistence;

/// <summary>Dapper-backed repository for ScoreCards read-model queries.</summary>
public sealed class ScoreCardsAggregateReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ScoreCardsAggregateReadDapperRepository> logger)
    : IScoreCardsRepository
{
    /// <inheritdoc/>
    public async Task<ScoreCardsItemDto?> GetByIdAsync(Guid scoreCardsId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for ScoreCards {ScoreCardsId} Tenant {TenantId}", scoreCardsId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ScoreCardsId", scoreCardsId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ScoreCardsItemDto>(
            "[Analytics].[usp_ScoreCardsAggregate_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScoreCardsItemDto>> GetScoreCardsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for ScoreCards tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ScoreCardsItemDto>(
            "[Analytics].[usp_ScoreCardsAggregate_GetRange]", parameters, cancellationToken);
    }
}
