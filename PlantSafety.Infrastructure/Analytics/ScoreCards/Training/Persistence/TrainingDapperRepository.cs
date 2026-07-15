// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards.Training
//  Layer:          Infrastructure
//  File:           TrainingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ScoreCards.Training read-model.
//                  Backs [Analytics].[ScoreCardsTraining] via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.ScoreCards.Training.Models;
using PlantSafety.Application.Analytics.ScoreCards.Training.Repositories;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Training.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrainingItemDto"/> queries.</summary>
public sealed class TrainingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<TrainingDapperRepository> logger)
    : ITrainingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrainingItemDto>> GetTrainingScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrainingScoresAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrainingItemDto>(
            "[Analytics].[usp_ScoreCardsTraining_List]", parameters, cancellationToken);
    }
}
