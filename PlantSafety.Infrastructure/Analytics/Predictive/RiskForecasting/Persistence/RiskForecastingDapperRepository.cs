// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Predictive.RiskForecasting
//  Layer:          Infrastructure
//  File:           RiskForecastingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RiskForecasting read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Predictive.RiskForecasting.Models;
using PlantSafety.Application.Analytics.Predictive.RiskForecasting.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Predictive.RiskForecasting.Persistence;

/// <summary>Dapper-backed repository for <see cref="RiskForecastingItemDto"/> queries.</summary>
public sealed class RiskForecastingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RiskForecastingDapperRepository> logger)
    : IRiskForecastingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RiskForecastingItemDto>> GetRiskForecastsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRiskForecastsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RiskForecastingItemDto>(
            "[Analytics].[usp_RiskForecasting_List]", parameters, cancellationToken);
    }
}
