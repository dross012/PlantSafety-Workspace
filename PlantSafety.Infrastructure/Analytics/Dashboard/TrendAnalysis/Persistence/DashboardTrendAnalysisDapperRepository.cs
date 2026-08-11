// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.TrendAnalysis
//  Layer:          Infrastructure
//  File:           DashboardTrendAnalysisDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Dashboard.TrendAnalysis read-model. All DB
//                  access via stored procedures. Named DashboardTrendAnalysisDapperRepository
//                  (rather than TrendAnalysisDapperRepository) to disambiguate from the
//                  top-level Analytics.TrendAnalysis.TrendAnalysisDapperRepository, which
//                  implements a different-but-identically-shaped ITrendAnalysisRepository
//                  interface in a different namespace.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Models;
using PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Dashboard.TrendAnalysis.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrendAnalysisItemDto"/> queries (Dashboard subdomain).</summary>
public sealed class DashboardTrendAnalysisDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<DashboardTrendAnalysisDapperRepository> logger)
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
        logger.LogDebug("GetTrendPointsAsync for Dashboard.TrendAnalysis tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrendAnalysisItemDto>(
            "[Analytics].[usp_DashboardTrendAnalysis_List]", parameters, cancellationToken);
    }
}
