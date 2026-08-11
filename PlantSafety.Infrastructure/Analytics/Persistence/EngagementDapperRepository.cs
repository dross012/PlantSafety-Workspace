// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Infrastructure
//  File:           EngagementDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the top-level Engagement analytics
//                  read-model (IEngagementRepository). All DB access via stored
//                  procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Models;
using PlantSafety.Application.Analytics.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Persistence;

/// <summary>Dapper-backed repository for <see cref="EngagementDataPointDto"/> queries.</summary>
public sealed class EngagementDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EngagementDapperRepository> logger)
    : IEngagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EngagementDataPointDto>> GetDataPointsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDataPointsAsync for Engagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EngagementDataPointDto>(
            "[Analytics].[usp_EngagementDataPoints_List]", parameters, cancellationToken);
    }
}
