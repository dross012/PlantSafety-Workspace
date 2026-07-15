// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture.EngagementScoring
//  Layer:          Infrastructure
//  File:           EngagementScoringDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EngagementScoring read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.EngagementScoring.Models;
using PlantSafety.Application.Analytics.Culture.EngagementScoring.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Culture.EngagementScoring.Persistence;

/// <summary>Dapper-backed repository for <see cref="EngagementScoringItemDto"/> queries.</summary>
public sealed class EngagementScoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EngagementScoringDapperRepository> logger)
    : IEngagementScoringRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EngagementScoringItemDto>> GetEngagementScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEngagementScoresAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EngagementScoringItemDto>(
            "[Analytics].[usp_EngagementScoring_List]", parameters, cancellationToken);
    }
}
