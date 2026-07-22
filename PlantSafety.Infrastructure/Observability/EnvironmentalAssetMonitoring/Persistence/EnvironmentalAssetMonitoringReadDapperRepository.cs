// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Infrastructure
//  File:           EnvironmentalAssetMonitoringReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EnvironmentalAssetMonitoring Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Models;
using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Repositories;

namespace PlantSafety.Infrastructure.Observability.EnvironmentalAssetMonitoring.Persistence;

/// <summary>Dapper-backed repository for EnvironmentalAssetMonitoring read-model queries.</summary>
public sealed class EnvironmentalAssetMonitoringReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EnvironmentalAssetMonitoringReadDapperRepository> logger)
    : IEnvironmentalAssetMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnvironmentalAssetMonitoringItemDto>> GetEnvironmentalAssetMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for EnvironmentalAssetMonitoring tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EnvironmentalAssetMonitoringItemDto>(
            "[Observability].[usp_EnvironmentalAssetMonitoring_GetRange]", parameters, cancellationToken);
    }
}
