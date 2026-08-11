// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.TrendAnalysis
//  Layer:          Infrastructure
//  File:           TrendAnalysisDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the top-level TrendAnalysis read-model.
//                  All DB access via stored procedures. Kept as
//                  TrendAnalysisDapperRepository (unprefixed) since it is the
//                  primary aggregate of this name; the Dashboard-subdomain sibling
//                  is disambiguated as DashboardTrendAnalysisDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.TrendAnalysis.Models;
using PlantSafety.Application.Analytics.TrendAnalysis.Repositories;

namespace PlantSafety.Infrastructure.Analytics.TrendAnalysis.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrendAnalysisItemDto"/> queries (top-level).</summary>
public sealed class TrendAnalysisDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<TrendAnalysisDapperRepository> logger)
    : ITrendAnalysisRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrendAnalysisItemDto>> GetTrendPointsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrendPointsAsync for TrendAnalysis tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrendAnalysisItemDto>(
            "[Analytics].[usp_TrendAnalysis_List]", parameters, cancellationToken);
    }
}
