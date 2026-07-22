// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Infrastructure
//  File:           PPEBehavioralMonitoringReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PPEBehavioralMonitoring Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Observability.PPEBehavioralMonitoring.Models;
using PlantSafety.Application.Observability.PPEBehavioralMonitoring.Repositories;

namespace PlantSafety.Infrastructure.Observability.PPEBehavioralMonitoring.Persistence;

/// <summary>Dapper-backed repository for PPEBehavioralMonitoring read-model queries.</summary>
public sealed class PPEBehavioralMonitoringReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PPEBehavioralMonitoringReadDapperRepository> logger)
    : IPPEBehavioralMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPEBehavioralMonitoringItemDto>> GetPPEBehavioralMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for PPEBehavioralMonitoring tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPEBehavioralMonitoringItemDto>(
            "[Observability].[usp_PPEBehavioralMonitoring_GetRange]", parameters, cancellationToken);
    }
}
