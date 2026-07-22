// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Infrastructure
//  File:           FacilityTrafficMonitoringReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the FacilityTrafficMonitoring Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Models;
using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Repositories;

namespace PlantSafety.Infrastructure.Observability.FacilityTrafficMonitoring.Persistence;

/// <summary>Dapper-backed repository for FacilityTrafficMonitoring read-model queries.</summary>
public sealed class FacilityTrafficMonitoringReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<FacilityTrafficMonitoringReadDapperRepository> logger)
    : IFacilityTrafficMonitoringRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FacilityTrafficMonitoringItemDto>> GetFacilityTrafficMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for FacilityTrafficMonitoring tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<FacilityTrafficMonitoringItemDto>(
            "[Observability].[usp_FacilityTrafficMonitoring_GetRange]", parameters, cancellationToken);
    }
}
