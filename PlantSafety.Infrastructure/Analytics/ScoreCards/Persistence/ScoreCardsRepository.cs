// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Infrastructure
//  File:           ScoreCardsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ScoreCards aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Persistence;

/// <summary>Dapper-backed repository for <see cref="ScoreCardsReadModel"/> queries.</summary>
public sealed class ScoreCardsRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ScoreCardsRepository> logger)
{
    /// <summary>Returns the ScoreCards read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<ScoreCardsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ScoreCards {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("ScoreCardsId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ScoreCardsReadModel>(
            $"[Analytics].[usp_ScoreCards_GetById]", parameters);
    }

    /// <summary>Returns all ScoreCards read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<ScoreCardsReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for ScoreCards tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<ScoreCardsReadModel>(
            $"[Analytics].[usp_ScoreCards_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
