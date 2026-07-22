// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Infrastructure
//  File:           EmergencyDrillPlanningReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencyDrillPlanning Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Repositories;

namespace PlantSafety.Infrastructure.Scheduling.EmergencyDrillPlanning.Persistence;

/// <summary>Dapper-backed repository for EmergencyDrillPlanning read-model queries.</summary>
public sealed class EmergencyDrillPlanningReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencyDrillPlanningReadDapperRepository> logger)
    : IEmergencyDrillPlanningRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmergencyDrillPlanningItemDto>> GetEmergencyDrillPlanningAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for EmergencyDrillPlanning tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencyDrillPlanningItemDto>(
            "[Scheduling].[usp_EmergencyDrillPlanning_GetRange]", parameters, cancellationToken);
    }
}
