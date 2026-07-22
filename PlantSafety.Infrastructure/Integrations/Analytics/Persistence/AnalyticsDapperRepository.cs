// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Integrations.Analytics
//  Layer:          Infrastructure
//  File:           AnalyticsDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Integrations.Analytics read-model. All DB access
//                  via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Integrations.Analytics.Models;
using PlantSafety.Application.Integrations.Analytics.Repositories;

namespace PlantSafety.Infrastructure.Integrations.Analytics.Persistence;

/// <summary>
/// Dapper-backed repository for Integrations.Analytics read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AnalyticsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AnalyticsDapperRepository> logger)
    : IAnalyticsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AnalyticsItemDto>> GetRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRecords for Integrations.Analytics tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AnalyticsItemDto>(
            "[Integrations].[usp_Analytics_GetRecords]", parameters, cancellationToken);
    }
}
