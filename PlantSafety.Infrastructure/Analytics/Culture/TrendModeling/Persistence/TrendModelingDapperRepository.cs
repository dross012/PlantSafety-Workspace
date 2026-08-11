// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture.TrendModeling
//  Layer:          Infrastructure
//  File:           TrendModelingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the TrendModeling read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.TrendModeling.Models;
using PlantSafety.Application.Analytics.Culture.TrendModeling.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Culture.TrendModeling.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrendModelingItemDto"/> queries.</summary>
public sealed class TrendModelingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<TrendModelingDapperRepository> logger)
    : ITrendModelingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrendModelingItemDto>> GetTrendModelsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrendModelsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrendModelingItemDto>(
            "[Analytics].[usp_TrendModeling_List]", parameters, cancellationToken);
    }
}
