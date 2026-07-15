// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ScoreCards.Compliance read-model.
//                  Backs [Analytics].[ScoreCardsCompliance] via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.ScoreCards.Compliance.Models;
using PlantSafety.Application.Analytics.ScoreCards.Compliance.Repositories;

namespace PlantSafety.Infrastructure.Analytics.ScoreCards.Compliance.Persistence;

/// <summary>Dapper-backed repository for <see cref="ComplianceItemDto"/> queries.</summary>
public sealed class ComplianceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ComplianceDapperRepository> logger)
    : IComplianceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceItemDto>> GetComplianceScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetComplianceScoresAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ComplianceItemDto>(
            "[Analytics].[usp_ScoreCardsCompliance_List]", parameters, cancellationToken);
    }
}
