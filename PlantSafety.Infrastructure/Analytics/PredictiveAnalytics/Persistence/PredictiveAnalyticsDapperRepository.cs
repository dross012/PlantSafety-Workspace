// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Infrastructure
//  File:           PredictiveAnalyticsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the top-level PredictiveAnalytics read-model
//                  (IPredictiveAnalyticsRepository). All DB access via stored procedures.
//                  Named PredictiveAnalyticsDapperRepository to coexist in the same
//                  namespace as the pre-existing unrelated scaffold class
//                  PredictiveAnalyticsRepository (which implements no interface and is
//                  out of scope for this pass).
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added GetByIdAsync (backs
//                  GetPredictiveAnalyticsByIdQuery), calling the new usp_PredictiveAnalytics_GetById
//                  proc which projects RowVersion as ETag. Additive; GetPredictionsAsync/
//                  usp_PredictiveAnalytics_List are unchanged.
// ============================================================================
using PlantSafety.Application.Analytics.PredictiveAnalytics.Models;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Repositories;

namespace PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Persistence;

/// <summary>Dapper-backed repository for <see cref="PredictiveAnalyticsItemDto"/> queries.</summary>
public sealed class PredictiveAnalyticsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveAnalyticsDapperRepository> logger)
    : IPredictiveAnalyticsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PredictiveAnalyticsItemDto>> GetPredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPredictionsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PredictiveAnalyticsItemDto>(
            "[Analytics].[usp_PredictiveAnalytics_List]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PredictiveAnalyticsItemDto?> GetByIdAsync(Guid predictionId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for PredictiveAnalytics {PredictionId} Tenant {TenantId}", predictionId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictionId", predictionId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PredictiveAnalyticsItemDto>(
            "[Analytics].[usp_PredictiveAnalytics_GetById]", parameters, cancellationToken);
    }
}
